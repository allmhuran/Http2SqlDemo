using System.Diagnostics;

namespace Coates.Demos.ProducerConsumer
{
   public static class Tests
   {
      public static async Task Cleanup() => await c.ClearAsync();

      public static async Task<long> SyncOne() => await ReadWriteAsync(c.WriteOneAsync);

      public static async Task<long> SyncMany() => await ReadWriteAsync(c.WriteManyAsync);

      public static async Task<long> SyncBulk() => await ReadWriteAsync(c.WriteBulkAsync);

      public static async Task<long> SyncTvp() => await ReadWriteAsync(c.InsertTvpAsync);

      public static async Task<long> SyncTvpMerge() => await ReadWriteAsync(c.MergeTvpAsync);

      public static int ObjectCount
      {
         get => server.ObjectCount;
         set => server.ObjectCount = value;
      }

      public static int? BatchSize { get; set; } = null;

      private static async Task<long> ReadWriteAsync(Func<IEnumerable<Dto>, Task> writer)
      {
         int skip = 0;
         int count = BatchSize ?? server.ObjectCount;
         sw.Restart();
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