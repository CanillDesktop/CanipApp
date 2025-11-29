namespace Shared.DTOs.Produtos;

public class ProdutosFiltroDTO
{
    public string? CodProduto { get; set; }
    public string? DescricaoSimples { get; set; }
    public string? NFe { get; set; }
    public int Categoria { get; set;}
    public DateTime? DataEntrega { get; set; }
    public DateTime? DataValidade { get; set; }
}
