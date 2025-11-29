using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Estoque
{
    public class ItemEstoqueDTO
    {
        public ItemEstoqueDTO(int idItem, string codItem, string? lote, int quantidade, DateTime dataEntrega, string? nfe, DateTime? dataValidade)
        {
            IdItem = idItem;
            CodItem = codItem;
            Lote = lote;
            Quantidade = quantidade;
            DataEntrega = dataEntrega;
            NFe = nfe;
            DataValidade = dataValidade;
        }
        public int IdItem { get; set; }

        [Display(Name = "Código do Item")]
        public string CodItem { get; set; } = string.Empty;

        [Display(Name = "Lote")]
        public string? Lote { get; set; } = string.Empty;

        [Display(Name = "Quantidade")]
        public int Quantidade { get; set; }

        [Display(Name = "Data de Entrega")]
        public DateTime DataEntrega { get; set; }

        [Display(Name = "NFe/DOC")]
        public string? NFe { get; set; } = string.Empty;

        [Display(Name = "Data de Validade")]
        public DateTime? DataValidade { get; set; }
    }
}
