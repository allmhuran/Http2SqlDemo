using System;
using System.Net.Http.Json;

namespace Coates.Demos.ProducerConsumer
{
   public class HttpProducer<T>
   {
      public async Task<IEnumerable<T>> ReadAsync(string url)
      {
         return (await _client.GetFromJsonAsync<IEnumerable<T>>(url) ?? Enumerable.Empty<T>());
      }
      private HttpClient _client = new HttpClient();
   }
}