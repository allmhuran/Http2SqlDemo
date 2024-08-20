using Microsoft.Data.SqlClient;
namespace ProducerConsumerDemo
{
   internal class SqlConsumer<T>(string tableName, string connectionSring = @"data source=coa-darc-sql17\dev_integration; initial catalog=scratch; integrated security=SSPI; TrustServerCertificate=true") where T : class
   { 

      public async Task ClearAsync()
      {
         using var con = new SqlConnection(connectionSring);
         
         await Dapper.Contrib.Extensions.SqlMapperExtensions.DeleteAllAsync<T>(con);
      }
      public async Task<int> CountAsync()
      {
         using var con = new SqlConnection(connectionSring);
         return await Dapper.SimpleCRUD.RecordCountAsync<T>(con);
      }

      public async Task WriteOneAsync(T item)
      {
         using var con = new SqlConnection(connectionSring);
         await Dapper.Contrib.Extensions.SqlMapperExtensions.InsertAsync(con, item);
      }

      public async Task WriteManyAsync(IEnumerable<T> items)
      {
         using var con = new SqlConnection(connectionSring);      
         await Dapper.Contrib.Extensions.SqlMapperExtensions.InsertAsync(con, items);
      }

   }
}
 