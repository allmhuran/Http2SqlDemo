using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ProducerConsumerDemo
{
   internal class HttpServer
   {
      public readonly int MaxObjectCount;

      public HttpServer(int maxObjectCount = 100)
      {
         MaxObjectCount = maxObjectCount;
         var builder = WebApplication.CreateBuilder();  
         builder.Logging.ClearProviders();
         var app = builder.Build();

         app.MapGet("/", async () => await GetAsync());
         app.MapGet("/paged", async (int skip, int count) => await GetAsync(skip, count));
         Task.Run(() => app.Run("http://localhost:8080"));
      }

      private async Task<string> GetAsync(int skip, int count) 
      {
         await Task.Delay(10);
         return JsonSerializer.Serialize
         (
            Enumerable.Range(skip + 1, count).Select(v => new Dto { i = v, s = v.ToString(), dt = DateTime.Now.AddDays(v) })
         );
      }

      private Task<string> GetAsync() => GetAsync(0, MaxObjectCount);
   }
}
