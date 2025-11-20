using Shared.DTOs.Estoque;
using Shared.DTOs.Produtos;
using Shared.Enums;

namespace Backend.Models.Produtos
{
    public class ProdutosModel : ItemComEstoqueBaseModel
    {
        public ProdutosModel() { }
        public string CodProduto { get; set; } = string.Empty;
        public string? DescricaoSimples { get; set; }
        public string? DescricaoDetalhada { get; set; }
        public UnidadeEnum Unidade { get; set; }
        public CategoriaEnum Categoria { get; set; }

        public static implicit operator ProdutosModel(ProdutosCadastroDTO dto)
        {
            return new ProdutosModel()
            {
                CodProduto = dto.CodProduto,
                DescricaoSimples = dto.DescricaoSimples,
                DescricaoDetalhada = dto.DescricaoDetalhada,
                Unidade = dto.Unidade,
                Categoria = dto.Categoria,
                ItemNivelEstoque = new()
                {
                    NivelMinimoEstoque = dto.NivelMinimoEstoque
                },
                ItensEstoque =
                [
                    new ItemEstoqueModel()
                    {
                        CodItem = dto.CodProduto,
                        DataEntrega = dto.DataEntrega,
                        DataValidade = dto.DataValidade,
                        Lote = dto.Lote,
                        NFe = dto.NFe,
                        Quantidade = dto.Quantidade
                    }
                ]
            };
        }

        public static implicit operator ProdutosCadastroDTO(ProdutosModel model)
        {
            var itemEstoque = model.ItensEstoque.FirstOrDefault();
            return new ProdutosCadastroDTO()
            {
                CodProduto = model.CodProduto,
                DescricaoSimples = model.DescricaoSimples,
                DescricaoDetalhada = model.DescricaoDetalhada,
                Unidade = model.Unidade,
                Categoria = model.Categoria,
                NivelMinimoEstoque = model.ItemNivelEstoque.NivelMinimoEstoque,
                DataEntrega = itemEstoque == null ? DateTime.Now : itemEstoque.DataEntrega,
                DataValidade = itemEstoque?.DataValidade,
                Lote = itemEstoque?.Lote,
                NFe = itemEstoque?.NFe,
                Quantidade = itemEstoque == null ? 0 : itemEstoque.Quantidade
            };
        }

        public static implicit operator ProdutosLeituraDTO(ProdutosModel model)
        {
            return new ProdutosLeituraDTO()
            {
                IdItem = model.IdItem,
                CodItem = model.CodProduto,
                NomeItem = model.DescricaoSimples,
                DescricaoDetalhada = model.DescricaoDetalhada,
                Unidade = model.Unidade,
                Categoria = model.Categoria,
                ItemNivelEstoque = model.ItemNivelEstoque,
                ItensEstoque = [.. model.ItensEstoque.Select(e => (ItemEstoqueDTO)e)]
            };
        }
    }
}
