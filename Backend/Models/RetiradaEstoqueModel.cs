using Amazon.DynamoDBv2.DataModel;
using Backend.Models.Interfaces;
using Shared.DTOs.Estoque;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    [DynamoDBTable("RetiradaEstoque")]
    public class RetiradaEstoqueModel : ISaveInsertDateModel
    {
        private DateTime _dataHoraInsercaoRegistro;

        [Key]
        [DynamoDBHashKey("id")]
        public int IdRetirada { get; set; }

        [DynamoDBProperty("CodItem")]
        public string CodItem { get; set; } = string.Empty;

        [DynamoDBProperty("NomeItem")]
        public string NomeItem { get; set; } = string.Empty;

        [DynamoDBProperty("Quantidade")]
        public int Quantidade { get; set; }

        [DynamoDBProperty("Lote")]
        public string Lote { get; set; } = string.Empty;

        [DynamoDBProperty("De")]
        public string De { get; set; } = string.Empty;

        [DynamoDBProperty("Para")]
        public string Para { get; set; } = string.Empty;

        [DynamoDBProperty("DataHoraInsercaoRegistro")]
        public DateTime DataHoraInsercaoRegistro
        {
            get => _dataHoraInsercaoRegistro;
            set
            {
                if (value == DateTime.MinValue)
                {
                    _dataHoraInsercaoRegistro = DateTime.UtcNow;
                }
                else
                {
                    _dataHoraInsercaoRegistro = value;
                }
            }
        }

        // ✅ NOVOS CAMPOS PARA SINCRONIZAÇÃO
        [DynamoDBProperty("DataAtualizacao")]
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        [DynamoDBProperty("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Conversões implícitas mantidas...
        public static implicit operator RetiradaEstoqueModel(RetiradaEstoqueDTO dto)
        {
            return new RetiradaEstoqueModel()
            {
                CodItem = dto.CodItem,
                NomeItem = dto.NomeItem,
                Lote = dto.Lote,
                De = dto.De,
                Para = dto.Para,
                Quantidade = dto.Quantidade,
                DataHoraInsercaoRegistro = dto.DataHoraInsercaoRegistro,
                DataAtualizacao = DateTime.UtcNow // ✅ Inicializar
            };
        }

        public static implicit operator RetiradaEstoqueDTO(RetiradaEstoqueModel model)
        {
            return new RetiradaEstoqueDTO()
            {
                CodItem = model.CodItem,
                NomeItem = model.NomeItem,
                Lote = model.Lote,
                De = model.De,
                Para = model.Para,
                Quantidade = model.Quantidade,
                DataHoraInsercaoRegistro = model.DataHoraInsercaoRegistro
            };
        }
    }
}