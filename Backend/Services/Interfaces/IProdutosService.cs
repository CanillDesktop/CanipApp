using Shared.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IProdutosService : IService<ProdutosDTO, string>
    {
        Task<IEnumerable<ProdutosDTO>> BuscarTodosAsync();
        Task<ProdutosDTO?> BuscarPorIdAsync(string id);
        Task<ProdutosDTO> CriarAsync(ProdutosDTO produtosDto);
        Task<ProdutosDTO?> AtualizarAsync(ProdutosDTO produtosDto);
        Task<bool> DeletarAsync(string id);
    }
}
