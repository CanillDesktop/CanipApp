using Amazon.DynamoDBv2.DataModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class ItemComEstoqueBaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DynamoDBHashKey("id")]
        public int IdItem { get; set; }

        [DynamoDBProperty("ItemNivelEstoque")]
        [NotMapped]
        public ItemNivelEstoqueModel? ItemNivelEstoqueDynamo { get; set; }

        [DynamoDBIgnore]
        public ItemNivelEstoqueModel ItemNivelEstoque { get; set; } = new();

        [DynamoDBProperty("ItensEstoque")]
        [NotMapped]
        public List<ItemEstoqueModel>? ItensEstoqueDynamo { get; set; }

        [DynamoDBIgnore]
        public ICollection<ItemEstoqueModel> ItensEstoque { get; set; } = [];

        [DynamoDBProperty("DataHoraInsercaoRegistro")]
        public DateTime DataHoraInsercaoRegistro { get; set; } = DateTime.UtcNow;
    }
}
