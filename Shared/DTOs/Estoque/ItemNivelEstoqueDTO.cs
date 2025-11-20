using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Estoque
{
    public class ItemNivelEstoqueDTO
    {
        public int IdItem { get; set; }

        [Display(Name = "Nível mínimo estoque")]
        public int NivelMinimoEstoque { get; set; }
    }
}
