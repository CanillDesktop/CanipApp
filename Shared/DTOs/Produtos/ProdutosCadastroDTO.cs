using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Shared.DTOs.Produtos
{
    public class ProdutosCadastroDTO
    {
        public ProdutosCadastroDTO() { }
        public ProdutosCadastroDTO(string? descricaoSimples, string? descricaoDetalhada, UnidadeEnum unidade, CategoriaEnum categoria, string? lote, DateTime dataEntrega,
            string? nFe, DateTime? dataValidade, int quantidade = 0, int nivelMinimoEstoque = 0)
        {
            CodProduto = GeraIdentificador(categoria);
            DescricaoSimples = descricaoSimples;
            DescricaoDetalhada = descricaoDetalhada;
            Unidade = unidade;
            Categoria = categoria;
            Lote = lote;
            DataEntrega = dataEntrega;
            Quantidade = quantidade;
            NFe = nFe;
            DataValidade = dataValidade;
            NivelMinimoEstoque = nivelMinimoEstoque;
        }

        public int IdProduto { get; set; }

        [Display(Name = "Código")]
        public string CodProduto { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        public string? DescricaoSimples { get; set; }

        [Display(Name = "Descrição Detalhada")]
        public string? DescricaoDetalhada { get; set; }

        [Display(Name = "Unidade")]
        public UnidadeEnum Unidade { get; set; }

        [Display(Name = "Categoria")]
        public CategoriaEnum Categoria { get; set; }

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

        [Display(Name = "Nível mínimo estoque")]
        public int NivelMinimoEstoque { get; set; }

        private static string GeraIdentificador(CategoriaEnum categoria)
        {
            var id = string.Empty;
            var catString = categoria.ToString();
            var categoriaCompostaSN = catString.Contains('_', StringComparison.CurrentCulture);

            if (catString.Length < 3)
                id += catString[..];
            else
                id += catString[..3];

            if (categoriaCompostaSN)
            {
                var i = catString.IndexOf('_', StringComparison.CurrentCulture);
                id += catString.Substring(i + 1, 1);
            }

            var guid = Guid.NewGuid().ToString().Replace("-", "");
            guid = Regex.Replace(guid, @"\D", "");

            id += guid;

            return id;
        }
    }
}
