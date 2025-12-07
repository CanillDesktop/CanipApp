using Shared.DTOs.Estoque;
using Shared.DTOs.Produtos;
using Amazon.DynamoDBv2.DataModel;
using Shared.Enums;

namespace Backend.Models.Produtos
{
    [DynamoDBTable("Produtos")]
    public class ProdutosModel : ItemComEstoqueBaseModel
    {
        public ProdutosModel() { }

        [DynamoDBProperty("CodProduto")]
        public string CodProduto { get; set; } = string.Empty;

        [DynamoDBProperty("DescricaoSimples")]
        public string? DescricaoSimples { get; set; }

        [DynamoDBProperty("DataEntrega")]
        public DateTime? DataEntrega { get; init; }

        [DynamoDBProperty("NFe")]
        public string? NFe { get; set; }

        [DynamoDBProperty("DescricaoDetalhada")]
        public string? DescricaoDetalhada { get; set; }

        [DynamoDBProperty("Unidade")]
        public UnidadeEnum Unidade { get; set; }

        [DynamoDBProperty("Categoria")]
        public CategoriaEnum Categoria { get; set; }

        [DynamoDBProperty("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        [DynamoDBProperty("DataAtualizacao")]
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        // Conversões implícitas mantidas (ajustar para usar propriedades corretas)...
        public static implicit operator ProdutosModel(ProdutosCadastroDTO dto)
        {
            return new ProdutosModel()
            {
                CodProduto = dto.CodProduto,
                DescricaoSimples = dto.DescricaoSimples,
                DescricaoDetalhada = dto.DescricaoDetalhada,
                Unidade = dto.Unidade,
                Categoria = dto.Categoria,

                // EF Core (SQLite)
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
                ],

                // DynamoDB (duplicado para serialização)
                ItemNivelEstoqueDynamo = new()
                {
                    NivelMinimoEstoque = dto.NivelMinimoEstoque
                },
                ItensEstoqueDynamo = new List<ItemEstoqueModel>
                {
                    new ItemEstoqueModel()
                    {
                        CodItem = dto.CodProduto,
                        DataEntrega = dto.DataEntrega,
                        DataValidade = dto.DataValidade,
                        Lote = dto.Lote,
                        NFe = dto.NFe,
                        Quantidade = dto.Quantidade
                    }
                }
            };
        }

        public static implicit operator ProdutosCadastroDTO(ProdutosModel model)
        {
            // Priorizar EF Core navigation properties se existirem, senão usar DynamoDB
            var itemEstoque = model.ItensEstoque?.FirstOrDefault()
                           ?? model.ItensEstoqueDynamo?.FirstOrDefault();

            var nivelEstoque = model.ItemNivelEstoque
                            ?? model.ItemNivelEstoqueDynamo;

            return new ProdutosCadastroDTO()
            {
                CodProduto = model.CodProduto,
                DescricaoSimples = model.DescricaoSimples,
                DescricaoDetalhada = model.DescricaoDetalhada,
                Unidade = model.Unidade,
                Categoria = model.Categoria,
                NivelMinimoEstoque = nivelEstoque?.NivelMinimoEstoque ?? 0,
                DataEntrega = itemEstoque?.DataEntrega ?? DateTime.UtcNow,
                DataValidade = itemEstoque?.DataValidade,
                Lote = itemEstoque?.Lote,
                NFe = itemEstoque?.NFe,
                Quantidade = itemEstoque?.Quantidade ?? 0
            };
        }

        public static implicit operator ProdutosLeituraDTO(ProdutosModel model)
        {
            // Priorizar EF Core, fallback para DynamoDB
            var itensEstoque = (model.ItensEstoque?.Any() == true
                ? model.ItensEstoque
                : model.ItensEstoqueDynamo) ?? new List<ItemEstoqueModel>();

            var nivelEstoque = model.ItemNivelEstoque ?? model.ItemNivelEstoqueDynamo!;

            return new ProdutosLeituraDTO()
            {
                IdItem = model.IdItem,
                CodItem = model.CodProduto,
                NomeItem = model.DescricaoSimples,
                DescricaoDetalhada = model.DescricaoDetalhada,
                Unidade = model.Unidade,
                Categoria = model.Categoria,
                ItemNivelEstoque = nivelEstoque,
                ItensEstoque = [.. itensEstoque.Select(e => (ItemEstoqueDTO)e)]
            };
        }
    }
}