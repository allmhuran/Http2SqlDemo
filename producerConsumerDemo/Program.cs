
using System.Diagnostics;

namespace ProducerConsumerDemo
{
   internal class Program
   {
      static async Task Main(string[] args)
      {
         var server = new HttpServer();
         var producer = new HttpProducer<Dto>();
         var consumer = new SqlConsumer<Dto>("");
         
         await consumer.ClearAsync();

         var sw = new Stopwatch(); 
         sw.Start();
         var results = await producer.ReadAsync(@"http://localhost:8080");
         foreach (var r in results) { await consumer.WriteOneAsync(r); }            
         sw.Stop();
         var rate = 1000f * (await consumer.CountAsync()) / sw.ElapsedMilliseconds;
         Console.WriteLine($"One by one wrote at {rate:f4} rows/s");
         
         await consumer.ClearAsync();

         sw.Restart();
         results = await producer.ReadAsync(@"http://localhost:8080");
         await consumer.WriteManyAsync(results);
         sw.Stop();
         rate = 1000f * (await consumer.CountAsync()) / sw.ElapsedMilliseconds;
         Console.WriteLine($"All at once wrote at {rate:f4} rows/s");



         
      }
   }
}
