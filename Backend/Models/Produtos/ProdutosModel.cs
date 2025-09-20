using Shared.DTOs;
using Shared.Enums;

namespace Backend.Models.Produtos
{
    internal class ProdutosModel
    {
        private string? _validade;

        public ProdutosModel() { }

        public string Codigo { get; init; } = string.Empty;
        public string? DescricaoSimples { get; set; }
        public DateTime DataEntrega { get; init; }
        public string? NFe { get; set; }
        public string? DescricaoDetalhada { get; set; }
        public UnidadeEnum Unidade { get; set; }
        public CategoriaEnum Categoria { get; set; }
        public int Quantidade { get; set; }
        public string? Validade
        {
            get => _validade ?? "indeterminado";
            set
            {
                if (DateTime.TryParse(value, out var _))
                    _validade = value;
            }
        }
        public DateTime DataHoraInsercaoRegistro { get; set; }
        public int EstoqueDisponivel { get; set; }

        public static implicit operator ProdutosModel(ProdutosDTO dto)
        {
            return new ProdutosModel()
            {
                Codigo = dto.Codigo,
                DescricaoSimples = dto.DescricaoSimples,
                DataEntrega = dto.DataEntrega,
                NFe = dto.NFe,
                DescricaoDetalhada = dto.DescricaoDetalhada,
                Unidade = dto.Unidade,
                Categoria = dto.Categoria,
                Quantidade = dto.Quantidade,
                Validade = dto.Validade,
                EstoqueDisponivel = dto.EstoqueDisponivel
            };
        }
    }
}
