namespace Shared.DTOs.Insumos;

public class InsumosFiltroDTO
{
    public string? CodInsumo { get; set; }
    public string? DescricaoSimplificada { get; set; }
    public string? NFe { get; set; }
    public int Unidade { get; set; }
    public DateTime? DataEntrega { get; set; }
    public DateTime? DataValidade { get; set; }
}
