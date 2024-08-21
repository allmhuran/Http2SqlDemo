using Dapper.Contrib.Extensions;

namespace Coates.Demos.ProducerConsumer
{
   [TableAttribute("PCD.Data")]
   public class Dto
   {
      [ExplicitKey]
      [FastMember.Ordinal(1)] public int i { get; set; }
      [FastMember.Ordinal(2)] public string s { get; set; }
      [FastMember.Ordinal(3)] public DateTime dt { get; set; }
   }
}