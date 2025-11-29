using Shared.DTOs.Estoque;
using Shared.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Estoque
{
    public class EstoqueItemModel : IEstoqueItem
    {
        public int IdItem { get; init; }

        [Display(Name = "Código")]
        public string CodItem { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        public string NomeItem { get; set; } = string.Empty;
        public ItemNivelEstoqueDTO? ItemNivelEstoque { get; set; }
        public ItemEstoqueDTO[]? ItensEstoque { get; set; }
    }
}
