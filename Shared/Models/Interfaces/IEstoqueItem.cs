using Shared.DTOs.Estoque;

namespace Shared.Models.Interfaces
{
    public interface IEstoqueItem
    {
        int IdItem { get; init; }
        string CodItem { get; set; }
        string NomeItem { get; set; }
        ItemNivelEstoqueDTO? ItemNivelEstoque { get; set; }
        ItemEstoqueDTO[]? ItensEstoque { get; set; }
    }
}
