using System.Diagnostics;

namespace Coates.Demos.ProducerConsumer
{
   internal class Program
   {
      private static void Main()
      {
         Console.SetWindowSize(100, 30);

         Next(1);

         Test(Tests.SyncOne);

         Next(1);

         Test(Tests.SyncOne);

         Next(100);

         Test(Tests.SyncOne);
         Test(Tests.SyncMany);

         Next(1000);

         Test(Tests.SyncOne);
         Test(Tests.SyncMany);
         Test(Tests.SyncBulk);
         Test(Tests.SyncTvp);
         Test(Tests.SyncTvpMerge);

         Next(200_000);

         Test(Tests.SyncBulk);
         Test(Tests.SyncTvp);
         Test(Tests.SyncTvpMerge);

         Next(400_000);

         Test(Tests.SyncBulk);
         Test(Tests.SyncTvp);
         Test(Tests.SyncTvpMerge);

         Next(200_000, 1000, 1000);

         Test(Tests.SyncBulk);
         Test(Tests.SyncTvp);
         Test(Tests.SyncTvpMerge);

         Next(200_000, 1000, 1000);

         Test(Tests.PipeBulk);
         Test(Tests.PipeTvp);
         Test(Tests.PipeTvpMerge);

         Next(200_000, 2000, 5000);

         Test(Tests.PipeBulk);
         Test(Tests.PipeTvp);
         Test(Tests.PipeTvpMerge);
         Test(Tests.StreamTvpMerge);

         Next(400_000, null, 5000);

         Test(Tests.StreamTvpMerge);

         Console.WriteLine("\nDone");
         Console.ReadKey();
      }

      private static void Next(int? count = null, int? readBatchSize = null, int? writeBatchSize = null)
      {
         Tests.TotalCount = count ?? Tests.TotalCount;
         Tests.ReadBatchSize = readBatchSize;
         Tests.WriteBatchSize = writeBatchSize;
         Console.WriteLine();
         Console.Write($"Process {count:n0} (read batch: {readBatchSize?.ToString() ?? "none"}, write batch: {writeBatchSize?.ToString() ?? "none"})");
         var (Left, Top) = Console.GetCursorPosition();
         Console.ReadKey();
         Console.SetCursorPosition(Left, Top);
         Console.WriteLine(" >");
         Console.WriteLine();
      }

      private static void Test(Func<long> test)
      {
         Task.Delay(4000).Wait();
         var allocBefore = GC.GetTotalAllocatedBytes();
         var ms = test();
         var tkb = GC.GetTotalMemory(false) / 1000f;
         var akb = (GC.GetTotalAllocatedBytes() - allocBefore) / 1000f;
         Console.WriteLine($"{test.Method.Name,-14} {Tests.TotalCount * 1000f / ms,8:n0}/s  (alloc {akb,9:n0}KB total {tkb,7:n0}KB)");
      }
   }
}
