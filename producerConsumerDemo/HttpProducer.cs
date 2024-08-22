using System;
using System.Net.Http.Json;

namespace Coates.Demos.ProducerConsumer
{
   public class HttpProducer<T>
   {
      public async Task<int> GetCount() => await _client.GetFromJsonAsync<int>($"http://localhost:8080/count");

      public async Task SetCount(int count)
      {
         var result = await _client.PostAsync($"http://localhost:8080/count/{count}", null);
         if (!result.IsSuccessStatusCode) throw new Exception(result.StatusCode.ToString());
      }

      public async Task<IEnumerable<T>> ReadAsync(int skip)
      {
         return await _client.GetFromJsonAsync<IEnumerable<T>>(@$"http://localhost:8080/get/{skip}/{BatchSize}") ?? Enumerable.Empty<T>();
      }

      public int BatchSize = 1;
      private HttpClient _client = new HttpClient();
   }
}