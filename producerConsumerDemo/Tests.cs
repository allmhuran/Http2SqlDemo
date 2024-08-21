using Coates.Demos.ProducerConsumer;
using Microsoft.Data.SqlClient.Server;
using System.Data;
using System.Diagnostics;

namespace Coates.Demos.ProducerConsumerApp
{
   public static class Tests
   {
      public static async Task Cleanup() => await c.ClearAsync();

      public static async Task<long> SyncOne() => await WriteAsync(c.WriteOneAsync);
      public static async Task<long> SyncMany() => await WriteAsync(c.WriteManyAsync);
      public static async Task<long> SyncBulk() => await WriteAsync(c.WriteBulkAsync);
      public static async Task<long> SyncTvp() => await WriteAsync(c.InsertvpAsync);
      public static async Task<long> SyncTvpMerge() => await WriteAsync(c.MergeTvpAsync);
      public static int ObjectCount
      {
         get => server.ObjectCount;
         set => server.ObjectCount = value;
      }
      public static int? BatchSize { get; set; } = null;
      private static async Task<long> WriteAsync(Func<IEnumerable<Dto>, Task> writer)
      {
         sw.Restart();
         int skip = 0;
         int count = BatchSize ?? server.ObjectCount;
         while (skip < server.ObjectCount)
         {
            var results = await p.ReadAsync(@$"http://localhost:8080/get/{skip}/{count}");
            await writer(results);
            skip += count;
         }
         sw.Stop();
         return sw.ElapsedMilliseconds;
      }
      private static SqlConsumer<Dto> c = new SqlConsumer<Dto>("PCD.Data");
      private static HttpProducer<Dto> p = new HttpProducer<Dto>();
      private static Stopwatch sw = new Stopwatch();
      private static HttpServer server = new HttpServer(100);
   }
}