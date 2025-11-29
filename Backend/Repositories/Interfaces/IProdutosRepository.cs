using Backend.Models.Produtos;
using Shared.DTOs.Produtos;

namespace Backend.Repositories.Interfaces
{
    public interface IProdutosRepository : IRepository<ProdutosModel>
    {
        Task<IEnumerable<ProdutosModel>> GetAsync(ProdutosFiltroDTO filtro);
    }
}
