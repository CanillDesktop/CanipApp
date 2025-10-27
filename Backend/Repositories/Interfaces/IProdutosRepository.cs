using Backend.Models.Produtos;
using Shared.DTOs;

namespace Backend.Repositories.Interfaces
{
    public interface IProdutosRepository : IRepository<ProdutosModel, string>
    {
        Task<IEnumerable<ProdutosModel>> GetAsync(ProdutosFiltroDTO filtro);
    }
}
