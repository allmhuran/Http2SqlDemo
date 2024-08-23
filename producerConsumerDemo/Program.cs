namespace Coates.Demos.ProducerConsumer
{
   internal class Program
   {
      static void Main(string[] args)
      {
         Console.SetWindowSize(90, 30);

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

         Next(100_000);

         Test(Tests.SyncBulk);
         Test(Tests.SyncTvp);
         Test(Tests.SyncTvpMerge);

         Next(100_000, 1000, 1000);

         Test(Tests.SyncBulk);
         Test(Tests.SyncTvp);
         Test(Tests.SyncTvpMerge);

         Next(100_000, 1000, 1000);

         Test(Tests.PipeBulk);
         Test(Tests.PipeTvp);
         Test(Tests.PipeTvpMerge);

         Next(100_000, 1000, 10000);

         Test(Tests.PipeBulk);
         Test(Tests.PipeTvp);
         Test(Tests.PipeTvpMerge);
         Test(Tests.StreamTvpMerge);

         Next(500_000, null, 10000);

         Test(Tests.StreamTvpMerge);

         Console.WriteLine("\nDone");
         Console.ReadKey();
      }

      static void Next(int? count = null, int? readBatchSize = null, int? writeBatchSize = null)
      {
         Tests.TotalCount = count ?? Tests.TotalCount;
         Tests.ReadBatchSize = readBatchSize;
         Tests.WriteBatchSize = writeBatchSize;
         Console.WriteLine();
         Console.Write($"Process {count:n0} (read batch: {readBatchSize?.ToString() ?? "none"}, write batch: {writeBatchSize?.ToString() ?? "none"})");
         var pos = Console.GetCursorPosition();
         Console.ReadKey();
         Console.SetCursorPosition(pos.Left, pos.Top);
         Console.WriteLine(" >");
         Console.WriteLine();
      }

      static void Test(Func<long> test)
      {
         GC.Collect();
         GC.WaitForPendingFinalizers();
         GC.Collect();
         var before = GC.GetTotalAllocatedBytes();
         var ms = test();
         var tkb = GC.GetTotalMemory(false) / 1000f;
         var akb = (GC.GetTotalAllocatedBytes() - before) / 1000f;
         Console.WriteLine($"{test.Method.Name,-14} {Tests.TotalCount * 1000f / ms,8:n0}/s   (alloc {akb,9:n0}KB total {tkb,7:n0}KB)");
      }
   }
}