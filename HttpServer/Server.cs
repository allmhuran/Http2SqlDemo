using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Coates.Demos.ProducerConsumer
{
   public class HttpServer
   {
      public HttpServer(int objectCount = 100)
      {
         ObjectCount = objectCount;
         var builder = WebApplication.CreateBuilder();
         builder.Logging.ClearProviders();
         _app = builder.Build();

         _app.MapGet("/get/{skip?}/{count?}", (int? skip, int? count) => Get(skip, count));
         _appTask = _app.RunAsync(@"http://localhost:8080");
      }

      public int ObjectCount { get; set; }

      private string Get(int? skip, int? count)
      {
         Task.Delay(20).GetAwaiter().GetResult();
         return JsonSerializer.Serialize
         (
            skip + count > ObjectCount
            ? Enumerable.Empty<Dto>()
            : Enumerable.Range(skip ?? 0 + 1, count ?? ObjectCount).Select(v => new Dto { i = v, s = v.ToString(), dt = DateTime.Now.AddDays(v) })
         );
      }

      private WebApplication _app;
      private Task _appTask;
      private bool disposedValue;
   }
}