namespace ProducerConsumerDemo
{
   [Dapper.Contrib.Extensions.TableAttribute("PCD.Data"), Dapper.Table("Data", Schema = "PCD")]
   public class Dto
   {      
      [Dapper.Contrib.Extensions.ExplicitKey, Dapper.Key, Dapper.Required]      
      [FastMember.Ordinal(1)] public int i {get; set;}      
      [FastMember.Ordinal(2)] public string s {get; set;}
      [FastMember.Ordinal(3)] public DateTime dt {get; set;}
   }
}
