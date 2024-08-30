using Dapper.Contrib.Extensions;
using System;
using System.Text.Json.Serialization;

namespace Coates.Demos.ProducerConsumer
{
   [Table("PCD.Data")]
   public class Dto
   {
      [FastMember.Ordinal(3)] public DateTime Dt { get; set; }

      [ExplicitKey]
      [FastMember.Ordinal(1)] public int I { get; set; }

      [FastMember.Ordinal(2)] public string S { get; set; }
   }

   [JsonSerializable(typeof(Dto))]
   public partial class DtoJsonContext : JsonSerializerContext
   {
   }
}
