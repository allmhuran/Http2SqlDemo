using System.Diagnostics;
using System.Threading.Channels;

namespace Coates.Demos.ProducerConsumer
{
   public static class Tests
   {
      public static long PipeBulk() => PipeAsync(c.WriteBulkAsync).Result;

      public static long PipeTvp() => PipeAsync(c.InsertTvpAsync).Result;

      public static long PipeTvpMerge() => PipeAsync(c.MergeTvpAsync).Result;

      public static long StreamTvpMerge() => StreamAsync().Result;

      public static long SyncBulk() => ReadWriteAsync(c.WriteBulkAsync).Result;

      public static long SyncMany() => ReadWriteAsync(c.WriteManyAsync).Result;

      public static long SyncOne() => ReadWriteAsync(c.WriteOneAsync).Result;

      public static long SyncTvp() => ReadWriteAsync(c.InsertTvpAsync).Result;

      public static long SyncTvpMerge() => ReadWriteAsync(c.MergeTvpAsync).Result;

      public static int? ReadBatchSize = 1;
      public static int TotalCount = 1;
      public static int? WriteBatchSize = 1;

      private static async Task<long> PipeAsync(Func<IEnumerable<Dto>, Task> writer)
      {
         //var opts = new BoundedChannelOptions(4096) { SingleReader = true, SingleWriter = true };
         //var channel = Channel.CreateBounded<Dto>(opts);
         var channel = Channel.CreateUnbounded<Dto>
         (
            new UnboundedChannelOptions
            {
               SingleReader = true,
               SingleWriter = true,
               AllowSynchronousContinuations = true
            }
         );
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

      private static async Task<long> StreamAsync()
      {
         var opts = new BoundedChannelOptions(4096) { SingleReader = true, SingleWriter = true };
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
               await foreach (var item in p.StreamAsync()) await channel.Writer.WriteAsync(item);
               channel.Writer.Complete();
            }
         );

         var t2 = c.WriteStreamAsync(channel.Reader.ReadAllAsync(), c.MergeTvpAsync);

         await Task.WhenAll(t1, t2);

         sw.Stop();
         return sw.ElapsedMilliseconds;
      }

      private static readonly SqlConsumer<Dto> c = new("PCD.Data");
      private static readonly HttpProducer<Dto> p = new();
      private static readonly Stopwatch sw = new();
   }
}
