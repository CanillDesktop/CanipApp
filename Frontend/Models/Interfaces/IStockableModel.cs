namespace Frontend.Models.Interfaces
{
    public interface IStockableModel
    {
        string? Lote { get; set; }
        int Quantidade { get; set; }
        DateTime DataEntrega { get; set; }
        string? NFe { get; set; }
        DateTime? DataValidade { get; set; }
        int NivelMinimoEstoque { get; set; }
    }
}
