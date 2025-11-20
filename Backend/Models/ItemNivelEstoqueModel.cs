using Backend.Models.Interfaces;
using Shared.DTOs.Estoque;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class ItemNivelEstoqueModel : ISaveInsertDateModel
    {
        public int IdItem { get; set; }
        public int NivelMinimoEstoque { get; set; }
        public DateTime DataHoraInsercaoRegistro { get; set; } = DateTime.Now;

        [JsonIgnore]
        public ItemComEstoqueBaseModel ItemBase { get; set; } = null!;

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
