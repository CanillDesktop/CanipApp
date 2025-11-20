using Backend.Models.Interfaces;
using Shared.DTOs.Estoque;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class ItemEstoqueModel : ISaveInsertDateModel
    {
        private string? _lote = string.Empty;

        public int IdItem { get; set; }
        public string CodItem { get; set; } = string.Empty;
        public string? Lote 
        {
            get => _lote;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _lote = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                }
                else
                {
                    _lote = value;
                }
            }
        }
        public int Quantidade { get; set; }
        public DateTime DataEntrega { get; set; }
        public string? NFe { get; set; } = string.Empty;
        public DateTime? DataValidade { get; set; }

        [JsonIgnore]
        public ItemComEstoqueBaseModel? ItemBase { get; set; }
        public DateTime DataHoraInsercaoRegistro { get; set; } = DateTime.Now;

        public static implicit operator ItemEstoqueDTO(ItemEstoqueModel model)
        {
            return new ItemEstoqueDTO
                (
                    model.IdItem,
                    model.CodItem,
                    model.Lote,
                    model.Quantidade,
                    model.DataEntrega,
                    model.NFe,
                    model.DataValidade
                );
        }

        public static implicit operator ItemEstoqueModel(ItemEstoqueDTO dto)
        {
            return new ItemEstoqueModel()
            {
                IdItem = dto.IdItem,
                CodItem = dto.CodItem,
                Lote = dto.Lote,
                Quantidade = dto.Quantidade,
                DataEntrega = dto.DataEntrega,
                NFe = dto.NFe,
                DataValidade = dto.DataValidade
            };
        }
    }
}
