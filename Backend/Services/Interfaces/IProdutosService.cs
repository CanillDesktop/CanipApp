using Shared.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IProdutosService : IService<ProdutosDTO, string>
    {
        Task<IEnumerable<ProdutosDTO>> BuscarTodosAsync(ProdutosFiltroDTO filtro);
    }
}
