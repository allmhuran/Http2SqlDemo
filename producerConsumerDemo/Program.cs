namespace Coates.Demos.ProducerConsumerApp
{
   internal class Program
   {
      static void Main(string[] args)
      {
         //Next(1);

         //Test(Tests.SyncOne);

         //Next(100);

         //Test(Tests.SyncOne);
         //Test(Tests.SyncMany);

         //Next(1000);

         //Test(Tests.SyncOne);
         //Test(Tests.SyncMany);
         //Test(Tests.SyncBulk);
         //Test(Tests.SyncTvp);
         //Test(Tests.SyncTvpMerge);

         //Next(10_000, 2000);

         //Test(Tests.SyncBulk);
         //Test(Tests.SyncTvp);
         //Test(Tests.SyncTvpMerge);

         //Next(10_000);

         //Test(Tests.SyncBulk);
         //Test(Tests.SyncTvp);
         //Test(Tests.SyncTvpMerge);

         //Next(100_000, 5000);

         //Test(Tests.SyncBulk);
         //Test(Tests.SyncTvp);
         //Test(Tests.SyncTvpMerge);

         //Next(100_000);

         //Test(Tests.SyncBulk);
         //Test(Tests.SyncTvp);
         //Test(Tests.SyncTvpMerge);

         Next(1_000_000, 5000);

         Test(Tests.SyncBulk);
         Test(Tests.SyncTvp);
         Test(Tests.SyncTvpMerge);

         Next();
      }

      static void Next(int? count = null, int? batchSize = null)
      {
         Tests.ObjectCount = count ?? Tests.ObjectCount;
         Tests.BatchSize = batchSize;
         Console.WriteLine();
         Console.Write($"Ready for {Tests.ObjectCount} ({(batchSize?.ToString() ?? "no batching")})");
         var pos = Console.GetCursorPosition();
         Console.ReadKey();
         Console.SetCursorPosition(pos.Left, pos.Top);
         Console.WriteLine(" >");
         Console.WriteLine();
      }

      static void Test(Func<Task<long>> test)
      {
         Tests.Cleanup().GetAwaiter().GetResult();
         GC.Collect();
         GC.WaitForPendingFinalizers();
         var ms = test().GetAwaiter().GetResult();
         Console.WriteLine($"{test.Method.Name,-20} wrote {Tests.ObjectCount,7} objects in {ms / 1000f,6:F2} s at {Tests.ObjectCount * 1000f / ms,9:F2} objects/s");
      }
   }
}