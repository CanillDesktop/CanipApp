namespace Shared.DTOs
{
    public class MedicamentosFiltroDTO
    {
        public string? IdMedicamento { get; set; }
        public string? NomeComercial { get; set; }
        public string? PrincipioAtivo { get; set; }
        public string? NumeroLote { get; set; }
        public string? Fabricante { get; set; }
        public int Categoria { get; set; }
        public DateTime? DataFabricacao { get; set; }
        public string? DataValidade { get; set; }
    }
}
