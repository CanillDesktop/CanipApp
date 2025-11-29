using Amazon.DynamoDBv2.DataModel;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class ItemComEstoqueBaseModel
    {
        [Key]
        [DynamoDBHashKey("id")]
        public int IdItem { get; set; }
        public ItemNivelEstoqueModel ItemNivelEstoque { get; set; } = new();
        public ICollection<ItemEstoqueModel> ItensEstoque { get; set; } = [];
        public DateTime DataHoraInsercaoRegistro { get; set; } = DateTime.Now;
    }
}
