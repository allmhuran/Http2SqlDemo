using Dapper.Contrib.Extensions;
using System;
using System.Text.Json.Serialization;

namespace Coates.Demos.ProducerConsumer
{
   [Table("PCD.Data")]
   public class Dto
   {
      [ExplicitKey]
      [FastMember.Ordinal(1)] public int i { get; set; }

      [FastMember.Ordinal(2)] public string s { get; set; }

      [FastMember.Ordinal(3)] public DateTime dt { get; set; }
   }

   [JsonSerializable(typeof(Dto))]
   public partial class DtoJsonContext : JsonSerializerContext
   {
   }
}