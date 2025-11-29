using Shared.DTOs.Estoque;
using Shared.Enums;
using Shared.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Insumos
{
    public class InsumosLeituraDTO : IEstoqueItem
    {
        public int IdItem { get; init; }

        [Display(Name = "Código")]
        public string CodItem {  get; set; } = string.Empty;

        [Display(Name = "Descrição Simplificada")]
        public string NomeItem { get; set; } = string.Empty;

        [Display(Name = "Descrição Detalhada")]
        public string DescricaoDetalhada { get; set; } = string.Empty;

        public UnidadeInsumosEnum Unidade { get; set; }

        public ItemNivelEstoqueDTO ItemNivelEstoque { get; set; } = new();

        public ItemEstoqueDTO[] ItensEstoque { get; set; } = [];
    }
}
