using Dapper;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;
using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using System.Data;

namespace Coates.Demos.ProducerConsumer
{
   public class SqlConsumer<T>(string tableName, string connectionSring = @"data source=coa-darc-sql17\dev_integration; initial catalog=scratch; integrated security=SSPI; TrustServerCertificate=true") where T : class
   {
      public async Task ClearAsync()
      {
         using var con = new SqlConnection(connectionSring);
         await con.DeleteAllAsync<T>();
      }

      public async Task WriteOneAsync(IEnumerable<T> items)
      {
         using var con = new SqlConnection(connectionSring);
         foreach (var item in items)
         {
            await con.InsertAsync(item);
         }
      }

      public async Task WriteManyAsync(IEnumerable<T> items)
      {
         using var con = new SqlConnection(connectionSring);
         await con.InsertAsync(items);
      }

      public async Task WriteBulkAsync(IEnumerable<T> items)
      {
         using var con = new SqlConnection(connectionSring);
         await con.OpenAsync();
         using var bcp = new SqlBulkCopy(con);
         var reader = ObjectReader.Create(items);
         bcp.DestinationTableName = tableName;
         await bcp.WriteToServerAsync(reader);
      }

      public async Task InsertvpAsync(IEnumerable<T> items) => await WriteTvpAsync(items, "pcd.InsertData");

      public async Task MergeTvpAsync(IEnumerable<T> items) => await WriteTvpAsync(items, "pcd.MergeData");

      private static IEnumerable<SqlDataRecord> map(IEnumerable<T> items)
      {
         foreach (var item in items)
         {
            var dto = item as Dto;
            r.SetInt32(0, dto.i);
            r.SetString(1, dto.s);
            r.SetDateTime(2, dto.dt);
            yield return r;
         }
      }
      private async Task WriteTvpAsync(IEnumerable<T> items, string dbProcName)
      {
         using var con = new SqlConnection(connectionSring);
         using var cmd = con.CreateCommand();
         cmd.CommandType = CommandType.StoredProcedure;
         cmd.CommandText = dbProcName;
         cmd.Parameters.Add
         (
            new SqlParameter
            {
               SqlDbType = SqlDbType.Structured,
               ParameterName = "@data",
               TypeName = "pcd.data",
               Direction = ParameterDirection.Input,
               Value = map(items)
            }
         );
         await con.OpenAsync();
         await cmd.ExecuteNonQueryAsync();
      }

      private static SqlDataRecord r = new SqlDataRecord
      (
         new SqlMetaData("i", SqlDbType.Int),
         new SqlMetaData("s", SqlDbType.VarChar, 32),
         new SqlMetaData("dt", SqlDbType.DateTime)
      );
   }
}