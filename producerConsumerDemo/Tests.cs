using System.Diagnostics;
using System.Threading.Channels;

namespace Coates.Demos.ProducerConsumer
{
   public static class Tests
   {
      public static long SyncOne() => ReadWriteAsync(c.WriteOneAsync).Result;

      public static long SyncMany() => ReadWriteAsync(c.WriteManyAsync).Result;

      public static long SyncBulk() => ReadWriteAsync(c.WriteBulkAsync).Result;

      public static long SyncTvp() => ReadWriteAsync(c.InsertTvpAsync).Result;

      public static long SyncTvpMerge() => ReadWriteAsync(c.MergeTvpAsync).Result;

      public static long StreamBulk() => StreamAsync(c.WriteBulkAsync).Result;

      public static long StreamTvp() => StreamAsync(c.InsertTvpAsync).Result;

      public static long StreamTvpMerge() => StreamAsync(c.MergeTvpAsync).Result;

      public static long GetObjectCount() => p.GetCount().Result;

      public static int TotalCount = 1;
      public static int? ReadBatchSize = 1;
      public static int? WriteBatchSize = 1;

      private static async Task<long> StreamAsync(Func<IEnumerable<Dto>, Task> writer)
      {
         var opts = new BoundedChannelOptions(3000) { SingleReader = true, SingleWriter = true };
         var channel = Channel.CreateBounded<Dto>(opts);
         await p.SetCount(TotalCount);
         await c.ClearAsync();
         p.BatchSize = ReadBatchSize ?? TotalCount;
         c.BatchSize = WriteBatchSize ?? TotalCount;

         sw.Restart();

         var t1 = Task.Run
         (
            async () =>
            {
               int skip = 0;
               while (true)
               {
                  var results = await p.ReadAsync(skip);
                  if (!results.Any())
                  {
                     channel.Writer.Complete();
                     break;
                  }
                  else
                  {
                     foreach (var item in results)
                     {
                        await channel.Writer.WriteAsync(item);
                     }
                  }
                  skip += p.BatchSize;
               }
            }
         );

         var t2 = c.WriteStreamAsync(channel.Reader.ReadAllAsync(), writer);

         await Task.WhenAll(t1, t2);
         sw.Stop();
         return sw.ElapsedMilliseconds;
      }

      private static async Task<long> ReadWriteAsync(Func<IEnumerable<Dto>, Task> write)
      {
         await p.SetCount(TotalCount);
         await c.ClearAsync();
         p.BatchSize = ReadBatchSize ?? TotalCount;
         c.BatchSize = WriteBatchSize ?? TotalCount;
         int skip = 0;
         sw.Restart();
         while (true)
         {
            var results = await p.ReadAsync(skip);
            if (!results.Any()) break;
            else await write(results);
            skip += p.BatchSize;
         }
         sw.Stop();
         return sw.ElapsedMilliseconds;
      }

      private static SqlConsumer<Dto> c = new SqlConsumer<Dto>("PCD.Data");
      private static HttpProducer<Dto> p = new HttpProducer<Dto>();
      private static Stopwatch sw = new Stopwatch();
   }
}