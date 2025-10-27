using Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class ProdutosDTO
    {
        public ProdutosDTO() { }
        public ProdutosDTO(string? descricaoSimples, DateTime? dataEntrega, string? nFe, string? descricaoDetalhada, UnidadeEnum unidade, CategoriaEnum categoria,
            int quantidade = 0, string? validade = null, int estoqueDisponivel = 0)
        {
            IdProduto = GeraIdentificador(categoria);
            DescricaoSimples = descricaoSimples;
            DataEntrega = dataEntrega;
            NFe = nFe;
            DescricaoDetalhada = descricaoDetalhada;
            Unidade = unidade;
            Categoria = categoria;
            Quantidade = quantidade;
            Validade = validade;
            EstoqueDisponivel = estoqueDisponivel;
        }

        [Display(Name = "Código")]
        public string IdProduto { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        public string? DescricaoSimples { get; set; }

        [Display(Name = "Data de Entrega")]
        public DateTime? DataEntrega { get; init; }

        [Display(Name = "NFe/DOC")]
        public string? NFe { get; set; }

        [Display(Name = "Descrição Detalhada")]
        public string? DescricaoDetalhada { get; set; }

        [Display(Name = "Unidade")]
        public UnidadeEnum Unidade { get; set; }

        [Display(Name = "Categoria")]
        public CategoriaEnum Categoria { get; set; }

        public int Quantidade { get; set; }

        [Display(Name = "Data de Validade")]
        public string? Validade { get; set; }

        [Display(Name = "Quantidade em Estoque")]
        public int EstoqueDisponivel { get; set; }

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

            id += Guid.NewGuid().ToString().Replace("-", "");

            return id;
        }
    }
}
