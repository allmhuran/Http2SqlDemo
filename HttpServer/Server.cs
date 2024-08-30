using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Coates.Demos.ProducerConsumer
{
   internal static class HttpServer
   {
      static HttpServer()
      {
         var builder = WebApplication.CreateBuilder();
         builder.Logging.ClearProviders();
         _app = builder.Build();
         _app.MapGet("/get/{skip?}/{count?}", (int? skip, int? count) => Get(skip, count));
         _app.MapPost("/count/{count}", (int count) => _objectCount = count);
      }

      public static void Run() => _app.Run(@"http://localhost:8080");

      private static string Get(int? skip, int? count)
      {
         Task.Delay(20).Wait();
         Console.WriteLine($"get skip = {skip,-7} count = {count,-7}");
         int rangeSkip = skip ?? 0;
         var rangeCount = Math.Min(count ?? _objectCount, _objectCount - rangeSkip);

         return JsonSerializer.Serialize
         (
            rangeSkip >= _objectCount
            ? []
            : Enumerable.Range(rangeSkip + 1, rangeCount).Select
            (
               v => new Dto
               {
                  I = v,
                  S = new string('s', Random.Shared.Next(8, 32)),
                  Dt = DateTime.Now.AddDays(v % 100)
               }
            )
         );
      }

      private static WebApplication _app;
      private static int _objectCount = 1;
   }

   internal class Server
   {
      private static void Main()
      {
         Console.SetWindowSize(40, 30);
         HttpServer.Run();
      }
   }
}
