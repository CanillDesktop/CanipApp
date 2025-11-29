using Shared.Enums;
using Shared.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using Shared.DTOs.Estoque;

namespace Shared.DTOs.Produtos
{
    public class ProdutosLeituraDTO : IEstoqueItem
    {
        public int IdItem { get; init; }

        [Display(Name = "Código")]
        public string CodItem { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        public string NomeItem { get; set; } = string.Empty;

        [Display(Name = "Descrição Detalhada")]
        public string? DescricaoDetalhada { get; set; }

        [Display(Name = "Unidade")]
        public UnidadeEnum Unidade { get; set; }

        [Display(Name = "Categoria")]
        public CategoriaEnum Categoria { get; set; }

        public ItemNivelEstoqueDTO ItemNivelEstoque { get; set; } = new();

        public ItemEstoqueDTO[] ItensEstoque { get; set; } = [];
    }
}
