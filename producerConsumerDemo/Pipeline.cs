using System.Threading.Channels;

namespace Coates.Demos.ProducerConsumer
{
   internal class Pipeline<T>(HttpProducer<T> p, SqlConsumer<T> c) where T : class
   {
   }
}