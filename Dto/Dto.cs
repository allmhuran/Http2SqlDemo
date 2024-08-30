using Dapper.Contrib.Extensions;

namespace Coates.Demos.ProducerConsumer
{
   [TableAttribute("PCD.Data")]
   public class Dto
   {
      [FastMember.Ordinal(3)] public DateTime Dt { get; set; }

      [ExplicitKey]
      [FastMember.Ordinal(1)] public int I { get; set; }

      [FastMember.Ordinal(2)] public string S { get; set; }
   }
}
