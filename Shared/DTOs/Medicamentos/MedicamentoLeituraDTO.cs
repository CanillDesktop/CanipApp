using Shared.DTOs.Estoque;
using Shared.Enums;
using Shared.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Medicamentos
{
    public class MedicamentoLeituraDTO : IEstoqueItem
    {
        public int IdItem { get; init; }

        [Display(Name = "Código")]
        public string CodItem { get; set; } = string.Empty;

        [Display(Name = "Nome Comercial")]
        public string NomeItem { get; set; } = string.Empty;
        public PrioridadeEnum Prioridade { get; set; }

        [Display(Name = "Descrição")]
        public string DescricaoMedicamento { get; set; } = string.Empty;

        [Display(Name = "Fórmula")]
        public string Formula { get; set; } = string.Empty;

        [Display(Name = "Público Alvo")]
        public PublicoAlvoMedicamentoEnum PublicoAlvo { get; set; }

        public ItemNivelEstoqueDTO ItemNivelEstoque { get; set; } = new();

        public ItemEstoqueDTO[] ItensEstoque { get; set; } = [];
    }
}
