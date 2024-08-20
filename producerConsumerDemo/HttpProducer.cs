using System.Net.Http.Json;

namespace ProducerConsumerDemo
{   
   internal class HttpProducer<T>()
   {
      
      public async Task<IEnumerable<T>> ReadAsync(string url)
      {
         var client = new HttpClient();        
         client.BaseAddress = new Uri(url);
         //var response = await client.GetAsync(url);
         //string s = await response.Content.ReadAsStringAsync();
         //return Enumerable.Empty<T>();
         return (await client.GetFromJsonAsync<IEnumerable<T>>(url) ?? Enumerable.Empty<T>());
      }          
   }
}
