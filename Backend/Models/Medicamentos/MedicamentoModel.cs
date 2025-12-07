using Amazon.DynamoDBv2.DataModel;
using Shared.DTOs.Estoque;
using Shared.DTOs.Medicamentos;
using Shared.Enums;

namespace Backend.Models.Medicamentos
{
    [DynamoDBTable("Medicamentos")]
    public class MedicamentosModel : ItemComEstoqueBaseModel
    {
        [DynamoDBProperty("CodMedicamento")]
        public string CodMedicamento { get; set; } = string.Empty;

        [DynamoDBProperty("Prioridade")]
        public PrioridadeEnum Prioridade { get; set; }

        [DynamoDBProperty("DescricaoMedicamento")]
        public required string DescricaoMedicamento { get; set; }

        [DynamoDBProperty("Formula")]
        public required string Formula { get; set; }

        [DynamoDBProperty("NomeComercial")]
        public required string NomeComercial { get; set; }

        [DynamoDBProperty("PublicoAlvo")]
        public PublicoAlvoMedicamentoEnum PublicoAlvo { get; set; }

        [DynamoDBProperty("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        [DynamoDBProperty("DataAtualizacao")]
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        // ============================================================================
        // CONVERSÃO: MedicamentoCadastroDTO → MedicamentosModel
        // ============================================================================
        public static implicit operator MedicamentosModel(MedicamentoCadastroDTO dto)
        {
            var itemEstoque = new ItemEstoqueModel()
            {
                CodItem = dto.CodMedicamento,
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

            return new MedicamentosModel()
            {
                CodMedicamento = dto.CodMedicamento,
                Prioridade = dto.Prioridade,
                DescricaoMedicamento = dto.DescricaoMedicamento,
                Formula = dto.Formula,
                NomeComercial = dto.NomeComercial,
                PublicoAlvo = dto.PublicoAlvo,

                // EF Core (SQLite)
                ItemNivelEstoque = nivelEstoque,
                ItensEstoque = [itemEstoque],

                // DynamoDB (duplicado para serialização)
                ItemNivelEstoqueDynamo = nivelEstoque,
                ItensEstoqueDynamo = [itemEstoque]
            };
        }

        // ============================================================================
        // CONVERSÃO: MedicamentosModel → MedicamentoCadastroDTO
        // ============================================================================
        public static implicit operator MedicamentoCadastroDTO(MedicamentosModel model)
        {
            // Priorizar navegações EF Core, fallback para DynamoDB
            var itemEstoque = model.ItensEstoque?.FirstOrDefault()
                           ?? model.ItensEstoqueDynamo?.FirstOrDefault();

            var nivelEstoque = model.ItemNivelEstoque
                            ?? model.ItemNivelEstoqueDynamo;

            return new MedicamentoCadastroDTO()
            {
                CodMedicamento = model.CodMedicamento,
                Prioridade = model.Prioridade,
                DescricaoMedicamento = model.DescricaoMedicamento,
                Lote = itemEstoque?.Lote,
                Quantidade = itemEstoque?.Quantidade ?? 0,
                DataEntrega = itemEstoque?.DataEntrega ?? DateTime.UtcNow,
                NFe = itemEstoque?.NFe,
                Formula = model.Formula,
                NomeComercial = model.NomeComercial,
                PublicoAlvo = model.PublicoAlvo,
                NivelMinimoEstoque = nivelEstoque?.NivelMinimoEstoque ?? 0,
                DataValidade = itemEstoque?.DataValidade
            };
        }

        // ============================================================================
        // CONVERSÃO: MedicamentosModel → MedicamentoLeituraDTO
        // ============================================================================
        public static implicit operator MedicamentoLeituraDTO(MedicamentosModel model)
        {
            // Priorizar EF Core, fallback para DynamoDB
            var itensEstoque = (model.ItensEstoque?.Any() == true
                ? model.ItensEstoque
                : model.ItensEstoqueDynamo) ?? new List<ItemEstoqueModel>();

            var nivelEstoque = model.ItemNivelEstoque ?? model.ItemNivelEstoqueDynamo!;

            return new MedicamentoLeituraDTO()
            {
                IdItem = model.IdItem,
                CodItem = model.CodMedicamento,
                NomeItem = model.NomeComercial,
                DescricaoMedicamento = model.DescricaoMedicamento,
                Formula = model.Formula,
                PublicoAlvo = model.PublicoAlvo,
                ItemNivelEstoque = nivelEstoque,
                ItensEstoque = [.. itensEstoque.Select(e => (ItemEstoqueDTO)e)]
            };
        }
    }
}