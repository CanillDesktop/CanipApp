using Shared.DTOs.Estoque;
using Shared.DTOs.Medicamentos;
using Shared.Enums;

namespace Backend.Models.Medicamentos
{
    
    public class MedicamentosModel : ItemComEstoqueBaseModel
    {
        public string CodMedicamento { get; set; } = string.Empty;
        public PrioridadeEnum Prioridade { get; set; }
        public required string DescricaoMedicamento { get; set; }
        public required string Formula { get; set; }
        public required string NomeComercial { get; set; }
        public PublicoAlvoMedicamentoEnum PublicoAlvo { get; set; }

        public static implicit operator MedicamentosModel(MedicamentoCadastroDTO dto)
        {
            return new MedicamentosModel()
            {
                CodMedicamento = dto.CodMedicamento,
                Prioridade = dto.Prioridade,
                DescricaoMedicamento = dto.DescricaoMedicamento,
                Formula = dto.Formula,
                NomeComercial = dto.NomeComercial,
                PublicoAlvo = dto.PublicoAlvo,
                ItemNivelEstoque = new()
                {
                    NivelMinimoEstoque = dto.NivelMinimoEstoque
                },
                ItensEstoque =
                [
                    new ItemEstoqueModel()
                    {
                        CodItem = dto.CodMedicamento,
                        DataEntrega = dto.DataEntrega,
                        DataValidade = dto.DataValidade,
                        Lote = dto.Lote,
                        NFe = dto.NFe,
                        Quantidade = dto.Quantidade
                    }
                ]
            };
        }

        public static implicit operator MedicamentoCadastroDTO(MedicamentosModel model)
        {
            var itemEstoque = model.ItensEstoque.FirstOrDefault();
            return new MedicamentoCadastroDTO()
            {
                CodMedicamento = model.CodMedicamento,
                Prioridade = model.Prioridade,
                DescricaoMedicamento = model.DescricaoMedicamento,
                Lote = itemEstoque?.Lote,
                Quantidade = itemEstoque == null ? 0 : itemEstoque!.Quantidade,
                DataEntrega = itemEstoque == null ? DateTime.Now : itemEstoque.DataEntrega,
                NFe = itemEstoque?.NFe,
                Formula = model.Formula,
                NomeComercial = model.NomeComercial,
                PublicoAlvo = model.PublicoAlvo,
                NivelMinimoEstoque = model.ItemNivelEstoque.NivelMinimoEstoque,
                DataValidade = itemEstoque?.DataValidade,
            };
        }

        public static implicit operator MedicamentoLeituraDTO(MedicamentosModel model)
        {
            return new MedicamentoLeituraDTO()
            {
                IdItem = model.IdItem,
                CodItem = model.CodMedicamento,
                NomeItem= model.NomeComercial,
                DescricaoMedicamento = model.DescricaoMedicamento,
                Formula = model.Formula,
                PublicoAlvo = model.PublicoAlvo,
                ItemNivelEstoque = model.ItemNivelEstoque,
                ItensEstoque = [.. model.ItensEstoque.Select(e => (ItemEstoqueDTO)e)]
            };
        }
    }
}



