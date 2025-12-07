using Backend.Models.Interfaces;
using Shared.DTOs.Estoque;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;

namespace Backend.Models
{
    // ✅ REMOVER [DynamoDBTable] - será aninhado
    public class ItemNivelEstoqueModel : ISaveInsertDateModel
    {
        [DynamoDBProperty("IdItem")]
        public int IdItem { get; set; }

        [DynamoDBProperty("NivelMinimoEstoque")]
        public int NivelMinimoEstoque { get; set; }

        [DynamoDBProperty("DataHoraInsercaoRegistro")]
        public DateTime DataHoraInsercaoRegistro { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        [DynamoDBIgnore] // DynamoDB ignora navegação
        public ItemComEstoqueBaseModel ItemBase { get; set; } = null!;

        // Conversões implícitas mantidas...
        public static implicit operator ItemNivelEstoqueDTO(ItemNivelEstoqueModel model)
        {
            return new ItemNivelEstoqueDTO()
            {
                IdItem = model.IdItem,
                NivelMinimoEstoque = model.NivelMinimoEstoque
            };
        }

        public static implicit operator ItemNivelEstoqueModel(ItemNivelEstoqueDTO dto)
        {
            return new ItemNivelEstoqueModel()
            {
                IdItem = dto.IdItem,
                NivelMinimoEstoque = dto.NivelMinimoEstoque
            };
        }
    }
}