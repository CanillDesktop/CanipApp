namespace Shared.DTOs.Estoque
{
    public class RetiradaEstoqueDTO
    {
        public string CodItem { get; set; } = string.Empty;
        public string NomeItem { get; set; } = string.Empty;
        public string Lote { get; set; } = string.Empty;
        public string De { get; set; } = string.Empty;
        public string Para { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public DateTime DataHoraInsercaoRegistro { get; set; }
    }
}
