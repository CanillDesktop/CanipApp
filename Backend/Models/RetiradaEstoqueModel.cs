using Amazon.DynamoDBv2.DataModel;
using Backend.Models.Interfaces;
using Shared.DTOs.Estoque;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class RetiradaEstoqueModel : ISaveInsertDateModel
    {
        private DateTime _dataHoraInsercaoRegistro;

        [Key]
        [DynamoDBHashKey("id")]
        public int IdRetirada { get; set; }
        public string CodItem { get; set; } = string.Empty;
        public string NomeItem { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public string Lote { get; set; } = string.Empty;
        public string De { get; set; } = string.Empty;
        public string Para { get; set; } = string.Empty;
        public DateTime DataHoraInsercaoRegistro 
        {
            get => _dataHoraInsercaoRegistro;
            set
            {
                if (value == DateTime.MinValue)
                {
                    _dataHoraInsercaoRegistro = DateTime.Now;
                }
                else
                {
                    _dataHoraInsercaoRegistro = value;
                }
            }
        }

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
                DataHoraInsercaoRegistro = dto.DataHoraInsercaoRegistro
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
