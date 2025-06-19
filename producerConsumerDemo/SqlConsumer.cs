using Dapper.Contrib.Extensions;
using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using System.Data;

namespace Coates.Demos.ProducerConsumer
{
   public class SqlConsumer<T>(string tableName, string connectionSring = @"data source=server; initial catalog=scratch; integrated security=SSPI; TrustServerCertificate=true") where T : class
   {
      public int BatchSize { get; set; }

      public async Task ClearAsync()
      {
         using var con = new SqlConnection(connectionSring);
         await con.DeleteAllAsync<T>();
      }

      public async Task InsertTvpAsync(IEnumerable<T> items) => await WriteTvpAsync(items, "pcd.InsertData");

      public async Task MergeTvpAsync(IEnumerable<T> items) => await WriteTvpAsync(items, "pcd.MergeData");

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

      public async Task WriteManyAsync(IEnumerable<T> items)
      {
         using var con = new SqlConnection(connectionSring);
         foreach (var batch in items.Chunk(BatchSize)) await con.InsertAsync(batch);
      }

      public async Task WriteOneAsync(IEnumerable<T> items)
      {
         using var con = new SqlConnection(connectionSring);
         foreach (var batch in items.Chunk(BatchSize))
         {
            foreach (var item in batch) await con.InsertAsync(item);
         }
      }

      public async Task WriteStreamAsync(IAsyncEnumerable<T> items, Func<IEnumerable<T>, Task> writer)
      {
         await foreach (var batch in BatchAsync(items))
         {
            await writer(batch);
         }
      }

      private static IEnumerable<SqlDataRecord> Map(IEnumerable<T> items)
      {
         foreach (var item in items)
         {
            var dto = item as Dto;
            r.SetInt32(0, dto.I);
            r.SetString(1, dto.S);
            r.SetDateTime(2, dto.Dt);
            yield return r;
         }
      }

      private async IAsyncEnumerable<T[]> BatchAsync(IAsyncEnumerable<T> data)
      {
         var batch = new T[BatchSize];
         int i = 0;
         await foreach (T item in data)
         {
            batch[i++] = item;
            if (i == BatchSize)
            {
               yield return batch;
               i = 0;
            }
         }
         if (i > 0) yield return batch[..i];
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
            tvp.Value = Map(batch);
            await cmd.ExecuteNonQueryAsync();
         }
      }

      private static readonly SqlDataRecord r = new
      (
         new SqlMetaData("i", SqlDbType.Int),
         new SqlMetaData("s", SqlDbType.VarChar, 32),
         new SqlMetaData("dt", SqlDbType.DateTime)
      );
   }
}
