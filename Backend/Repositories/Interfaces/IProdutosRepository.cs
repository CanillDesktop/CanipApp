using Backend.Models.Produtos;
using Shared.DTOs;
using System.Linq.Expressions;

namespace Backend.Repositories.Interfaces
{
    public interface IProdutosRepository 
    {
        Task<IEnumerable<Produtos>> BuscarTodosAsync(Expression<Func<Produtos, bool>>? filter = null);

        Task<Produtos?> BuscarPorIdAsync(string id);

        Task<Produtos> CriarAsync(Produtos model);

        Task<Produtos?> AtualizarAsync(Produtos model);

        Task<bool> DeletarAsync(string id);

    }
}
