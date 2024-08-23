using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Coates.Demos.ProducerConsumer
{
   static class HttpServer
   {
      static HttpServer()
      {
         var builder = WebApplication.CreateBuilder();
         builder.Logging.ClearProviders();
         _app = builder.Build();
         _app.MapGet("/get/{skip?}/{count?}", (int? skip, int? count) => Get(skip, count));
         _app.MapGet("/count", () => TypedResults.Ok(_objectCount));
         _app.MapPost("/count/{count}", (int count) => _objectCount = count);
      }

      public static void Run() => _app.Run(@"http://localhost:8080");

      private static string Get(int? skip, int? count)
      {
         Task.Delay(10).Wait();
         Console.WriteLine($"get skip = {skip,-7} count = {count,-7}");
         Task.Delay(10).GetAwaiter().GetResult();
         int rangeSkip = skip ?? 0;
         var rangeCount = Math.Min(count ?? _objectCount, _objectCount - rangeSkip);

         return JsonSerializer.Serialize
         (
            rangeSkip >= _objectCount
            ? Enumerable.Empty<Dto>()
            : Enumerable.Range(rangeSkip + 1, rangeCount).Select
            (
               v => new Dto
               {
                  i = v,
                  s = new string('s', Random.Shared.Next(8, 32)),
                  dt = DateTime.Now.AddDays(v % 100)
               }
            )
         );
      }

      private static int _objectCount = 1;
      private static WebApplication _app;
   }

   internal class Server
   {
      static void Main(string[] args)
      {
         Console.SetWindowSize(40, 30);
         HttpServer.Run();
      }
   }
}