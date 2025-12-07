using Shared.DTOs.Estoque;
using Shared.DTOs.Insumos;
using Amazon.DynamoDBv2.DataModel;
using Shared.Enums;

namespace Backend.Models.Insumos
{
    [DynamoDBTable("Insumos")]
    public class InsumosModel : ItemComEstoqueBaseModel
    {
        [DynamoDBProperty("CodInsumo")]
        public string CodInsumo { get; set; } = string.Empty;

        [DynamoDBProperty("DescricaoSimplificada")]
        public required string DescricaoSimplificada { get; set; }

        [DynamoDBProperty("DescricaoDetalhada")]
        public required string DescricaoDetalhada { get; set; }

        [DynamoDBProperty("Unidade")]
        public UnidadeInsumosEnum Unidade { get; set; }

        [DynamoDBProperty("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        [DynamoDBProperty("DataAtualizacao")]
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        // ============================================================================
        // CONVERSÃO: InsumosCadastroDTO → InsumosModel
        // ============================================================================
        public static implicit operator InsumosModel(InsumosCadastroDTO dto)
        {
            var itemEstoque = new ItemEstoqueModel()
            {
                CodItem = dto.CodInsumo,
                DataEntrega = dto.DataEntrega,
                DataValidade = dto.DataValidade,
                Lote = dto.Lote,
                NFe = dto.NFe,
                Quantidade = dto.Quantidade
            };

            var nivelEstoque = new ItemNivelEstoqueModel()
            {
                NivelMinimoEstoque = dto.NivelMinimoEstoque
            };

            return new InsumosModel()
            {
                CodInsumo = dto.CodInsumo,
                DescricaoSimplificada = dto.DescricaoSimplificada,
                DescricaoDetalhada = dto.DescricaoDetalhada,
                Unidade = dto.Unidade,

                // EF Core (SQLite)
                ItemNivelEstoque = nivelEstoque,
                ItensEstoque = [itemEstoque],

                // DynamoDB (duplicado para serialização)
                ItemNivelEstoqueDynamo = nivelEstoque,
                ItensEstoqueDynamo = [itemEstoque]
            };
        }

        // ============================================================================
        // CONVERSÃO: InsumosModel → InsumosCadastroDTO
        // ============================================================================
        public static implicit operator InsumosCadastroDTO(InsumosModel model)
        {
            // Priorizar navegações EF Core, fallback para DynamoDB
            var itemEstoque = model.ItensEstoque?.FirstOrDefault()
                           ?? model.ItensEstoqueDynamo?.FirstOrDefault();

            var nivelEstoque = model.ItemNivelEstoque
                            ?? model.ItemNivelEstoqueDynamo;

            return new InsumosCadastroDTO()
            {
                CodInsumo = model.CodInsumo,
                DescricaoSimplificada = model.DescricaoSimplificada,
                DescricaoDetalhada = model.DescricaoDetalhada,
                Lote = itemEstoque?.Lote,
                Quantidade = itemEstoque?.Quantidade ?? 0,
                DataEntrega = itemEstoque?.DataEntrega ?? DateTime.UtcNow,
                NFe = itemEstoque?.NFe,
                NivelMinimoEstoque = nivelEstoque?.NivelMinimoEstoque ?? 0,
                DataValidade = itemEstoque?.DataValidade,
                Unidade = model.Unidade
            };
        }

        // ============================================================================
        // CONVERSÃO: InsumosModel → InsumosLeituraDTO
        // ============================================================================
        public static implicit operator InsumosLeituraDTO(InsumosModel model)
        {
            // Priorizar EF Core, fallback para DynamoDB
            var itensEstoque = (model.ItensEstoque?.Any() == true
                ? model.ItensEstoque
                : model.ItensEstoqueDynamo) ?? new List<ItemEstoqueModel>();

            var nivelEstoque = model.ItemNivelEstoque ?? model.ItemNivelEstoqueDynamo!;

            return new InsumosLeituraDTO()
            {
                IdItem = model.IdItem,
                CodItem = model.CodInsumo,
                NomeItem = model.DescricaoSimplificada,
                DescricaoDetalhada = model.DescricaoDetalhada,
                Unidade = model.Unidade,
                ItemNivelEstoque = nivelEstoque,
                ItensEstoque = [.. itensEstoque.Select(e => (ItemEstoqueDTO)e)]
            };
        }
    }
}