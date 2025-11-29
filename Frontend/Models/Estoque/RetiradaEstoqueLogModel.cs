using Shared.DTOs.Estoque;

namespace Frontend.Models.Estoque
{
    public class RetiradaEstoqueLogModel
    {
        public string CodItem { get; set; } = string.Empty;
        public string NomeItem { get; set; } = string.Empty;
        public string Lote { get; set; } = string.Empty;
        public string De { get; set; } = string.Empty;
        public string Para { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public DateTime DataHoraInsercaoRegistro { get; set; }

        public static implicit operator RetiradaEstoqueLogModel(RetiradaEstoqueDTO dto)
        {
            return new RetiradaEstoqueLogModel()
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

        public static implicit operator RetiradaEstoqueDTO(RetiradaEstoqueLogModel model)
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
