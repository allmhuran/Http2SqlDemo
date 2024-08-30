using System;
using System.Net.Http.Json;
using System.Text.Json;

namespace Coates.Demos.ProducerConsumer
{
   public class HttpProducer<T>
   {
      //public async Task<int> GetCount() => await _client.GetFromJsonAsync<int>($"http://localhost:8080/count");

      public async Task<IEnumerable<T>> ReadAsync(int skip) => await _client.GetFromJsonAsync<IEnumerable<T>>(@$"http://localhost:8080/get/{skip}/{BatchSize}") ?? [];

      public async Task SetCount(int count)
      {
         var result = await _client.PostAsync($"http://localhost:8080/count/{count}", null);
         if (!result.IsSuccessStatusCode) throw new Exception(result.StatusCode.ToString());
      }

      public async IAsyncEnumerable<T> StreamAsync()
      {
         var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:8080/get");
         var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
         var stream = await response.Content.ReadAsStreamAsync();
         var opts = new JsonSerializerOptions { DefaultBufferSize = 4096 };
         await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<T>(stream, opts))
         {
            yield return item;
         }
      }

      public int BatchSize = 1;
      private readonly HttpClient _client = new();
   }
}
