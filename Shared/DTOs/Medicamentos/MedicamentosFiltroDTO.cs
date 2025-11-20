namespace Shared.DTOs.Medicamentos
{
    public class MedicamentosFiltroDTO
    {
        public string? CodMedicamento { get; set; }
        public string? NomeComercial { get; set; }
        public string? Formula { get; set; }
        public string? DescricaoMedicamento { get; set; }
        public string? NFe { get; set; }
        public int Prioridade { get; set; }
        public int PublicoAlvo { get; set; }
        public DateTime? DataEntrega { get; set; }
        public DateTime? DataValidade { get; set; }
    }
}
