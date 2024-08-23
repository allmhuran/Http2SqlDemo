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
         foreach (var batch in items.Chunk(BatchSize))
         {
            foreach (var item in batch) await con.InsertAsync(item);
         }
      }

      public async Task WriteManyAsync(IEnumerable<T> items)
      {
         using var con = new SqlConnection(connectionSring);
         foreach (var batch in items.Chunk(BatchSize)) await con.InsertAsync(batch);
      }

      public async Task WriteBulkAsync(IEnumerable<T> items)
      {
         using var con = new SqlConnection(connectionSring);
         await con.OpenAsync();
         using var bcp = new SqlBulkCopy(con);
         foreach (var batch in items.Chunk(BatchSize))
         {
            var reader = ObjectReader.Create(batch);
            bcp.DestinationTableName = tableName;
            await bcp.WriteToServerAsync(reader);
         }
      }

      public async Task InsertTvpAsync(IEnumerable<T> items) => await WriteTvpAsync(items, "pcd.InsertData");

      public async Task MergeTvpAsync(IEnumerable<T> items) => await WriteTvpAsync(items, "pcd.MergeData");

      public async Task WriteStreamAsync(IAsyncEnumerable<T> items, Func<IEnumerable<T>, Task> writer)
      {
         await foreach (var batch in BatchAsync(items))
         {
            await writer(batch);
         }
      }

      public int BatchSize = 1;

      private async IAsyncEnumerable<T[]> BatchAsync(IAsyncEnumerable<T> data)
      {
         List<T> batch = new(BatchSize);

         await foreach (T item in data)
         {
            batch.Add(item);
            if (batch.Count == BatchSize)
            {
               yield return batch.ToArray();
               batch.Clear();
            }
         }
         if (batch.Count > 0) yield return batch.ToArray();
      }

      private IEnumerable<SqlDataRecord> map(IEnumerable<T> items)
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
         var tvp = new SqlParameter
         {
            SqlDbType = SqlDbType.Structured,
            ParameterName = "@data",
            TypeName = "pcd.data",
            Direction = ParameterDirection.Input
         };
         cmd.Parameters.Add(tvp);

         await con.OpenAsync();

         foreach (var batch in items.Chunk(BatchSize))
         {
            tvp.Value = map(batch);
            await cmd.ExecuteNonQueryAsync();
         }
      }

      private static SqlDataRecord r = new SqlDataRecord
      (
         new SqlMetaData("i", SqlDbType.Int),
         new SqlMetaData("s", SqlDbType.VarChar, 32),
         new SqlMetaData("dt", SqlDbType.DateTime)
      );
   }
}