using Backend.Models.Interfaces;
using Shared.DTOs.Estoque;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;

namespace Backend.Models
{
    // ✅ REMOVER [DynamoDBTable] - este model será aninhado, não é tabela raiz
    public class ItemEstoqueModel : ISaveInsertDateModel
    {
        private string? _lote = string.Empty;

        [DynamoDBProperty("IdItem")]
        public int IdItem { get; set; }

        [DynamoDBProperty("CodItem")]
        public string CodItem { get; set; } = string.Empty;

        [DynamoDBProperty("Lote")]
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

        [DynamoDBProperty("Quantidade")]
        public int Quantidade { get; set; }

        [DynamoDBProperty("DataEntrega")]
        public DateTime DataEntrega { get; set; }

        [DynamoDBProperty("NFe")]
        public string? NFe { get; set; } = string.Empty;

        [DynamoDBProperty("DataValidade")]
        public DateTime? DataValidade { get; set; }

        [JsonIgnore]
        [DynamoDBIgnore] // DynamoDB ignora navegação
        public ItemComEstoqueBaseModel? ItemBase { get; set; }

        [DynamoDBProperty("DataHoraInsercaoRegistro")]
        public DateTime DataHoraInsercaoRegistro { get; set; } = DateTime.UtcNow;

        // Conversões implícitas mantidas...
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