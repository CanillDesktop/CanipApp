using Shared.DTOs.Medicamentos;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Frontend.Models.Medicamentos;

public class MedicamentosFiltroModel
{
    [Display(Name = "Código")]
    public string CodMedicamento { get; set; } = string.Empty;

    [Display(Name = "Nome Comercial")]
    public string NomeComercial { get; set; } = string.Empty;

    [Display(Name = "Fórmula")]
    public string Formula { get; set; } = string.Empty;

    [Display(Name = "Descrição")]
    public string DescricaoMedicamento { get; set; } = string.Empty;

    [Display(Name = "Data de Entrega")]
    public string DataEntrega { get; set; } = string.Empty;

    [Display(Name = "NFe")]
    public string NFe { get; set; } = string.Empty;

    public string Prioridade { get; set; } = string.Empty;

    [Display(Name = "Público Alvo")]
    public string PublicoAlvo { get; set; } = string.Empty;

    [Display(Name = "Data de Validade")]
    public string DataValidade { get; set; } = string.Empty;


    public static implicit operator MedicamentosFiltroDTO(MedicamentosFiltroModel model)
    {
        return new MedicamentosFiltroDTO()
        {
            CodMedicamento = model.CodMedicamento,
            NomeComercial = model.NomeComercial,
            Formula = model.Formula,
            DescricaoMedicamento = model.DescricaoMedicamento,
            NFe = model.NFe,
            Prioridade = (int)Enum.Parse(typeof(PrioridadeEnum), model.Prioridade),
            PublicoAlvo = (int)Enum.Parse(typeof(PublicoAlvoMedicamentoEnum), model.PublicoAlvo),
            DataEntrega = DateTime.ParseExact(model.DataEntrega, "dd/MM/yyyy", CultureInfo.CurrentCulture),
            DataValidade = DateTime.ParseExact(model.DataValidade, "dd/MM/yyyy", CultureInfo.CurrentCulture)
        };
    }
}
