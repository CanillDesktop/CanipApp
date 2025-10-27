namespace Shared.DTOs;

public class ProdutosFiltroDTO
{
    public string? DescricaoSimples { get; set; }
    public string? NFe { get; set; }
    public int Categoria { get; set;}
    public DateTime? DataEntrega { get; set; }
    public string? DataValidade { get; set; }
}
