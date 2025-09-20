using Shared.Enums;

namespace Shared.DTOs
{
    public class ProdutosDTO
    {
        public ProdutosDTO(string? descricaoSimples, DateTime dataEntrega, string? nFe, string? descricaoDetalhada, UnidadeEnum unidade, CategoriaEnum categoria,
            int quantidade = 0, string? validade = null, int estoqueDisponivel = 0)
        {
            Codigo = GeraCodigo(categoria);
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

        public string Codigo { get; init; } = string.Empty;
        public string? DescricaoSimples { get; set; }
        public DateTime DataEntrega { get; init; }
        public string? NFe { get; set; }
        public string? DescricaoDetalhada { get; set; }
        public UnidadeEnum Unidade { get; set; }
        public CategoriaEnum Categoria { get; set; }
        public int Quantidade { get; set; }
        public string? Validade { get; set; }
        public int EstoqueDisponivel { get; set; }

        private static string GeraCodigo(CategoriaEnum categoria)
        {
            var codigo = string.Empty;
            var catString = categoria.ToString();
            var categoriaCompostaSN = catString.Contains('_', StringComparison.CurrentCulture);

            if (catString.Length < 3)
                codigo += catString[..];
            else
                codigo += catString[..3];

            if (categoriaCompostaSN)
            {
                var i = catString.IndexOf('_', StringComparison.CurrentCulture);
                codigo += catString.Substring(i + 1, 1);
            }

            codigo += Guid.NewGuid().ToString().Replace("-", "");

            return codigo;
        }
    }
}
}
