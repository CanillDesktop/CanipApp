using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Shared.DTOs.Insumos
{
    public class InsumosCadastroDTO
    {
        public InsumosCadastroDTO() 
        {
        }
        public InsumosCadastroDTO(string descricaoSimplificada, string? descricaoDetalhada, string? lote, int quantidade, DateTime dataEntrega, string? nFE, UnidadeInsumosEnum unidade, 
            DateTime? dataValidade, int nivelMinimoEstoque)
        {
            CodInsumo = GeraIdentificador();
            DescricaoSimplificada = descricaoSimplificada;
            DescricaoDetalhada = descricaoDetalhada;
            Lote = lote;
            Quantidade = quantidade;
            DataEntrega = dataEntrega;
            NFe = nFE;
            Unidade = unidade;
            DataValidade = dataValidade;
            NivelMinimoEstoque = nivelMinimoEstoque;
        }

        public int CodigoId { get; set; }

        [Display(Name = "Código")]
        public string CodInsumo { get; set; } = string.Empty;

        [Display(Name = "Descrição Simplificada")]
        public string DescricaoSimplificada { get; set; } = string.Empty;

        [Display(Name = "Descrição Detalhada")]
        public string DescricaoDetalhada { get; set; } = string.Empty;

        [Display(Name = "Lote")]
        public string? Lote { get; set; } = string.Empty;

        [Display(Name = "Quantidade")]
        public int Quantidade { get; set; }

        [Display(Name = "Data de Entrega")]
        public DateTime DataEntrega { get; set; }

        [Display(Name = "NFe/DOC")]
        public string? NFe { get; set; } = string.Empty;

        [Display(Name = "Unidade")]
        public UnidadeInsumosEnum Unidade { get; set; }

        [Display(Name = "Data de Validade")]
        public DateTime? DataValidade { get; set; }

        [Display(Name = "Nível mínimo estoque")]
        public int NivelMinimoEstoque { get; set; }

        private static string GeraIdentificador()
        {
            var id = "INS";

            var guid = Guid.NewGuid().ToString().Replace("-", "");
            guid = Regex.Replace(guid, @"\D", "");

            id += guid;

            return id;
        }
    }
}


