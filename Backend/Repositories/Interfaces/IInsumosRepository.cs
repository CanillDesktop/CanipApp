using Backend.Models.Insumos;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Repositories.Interfaces
{
   
    public interface IInsumosRepository
    {
        Task<IEnumerable<InsumosModel>> BuscarTodosAsync(Expression<Func<InsumosModel, bool>>? predicate = null);
        Task<InsumosModel?> BuscarPorIdAsync(int id);
        Task<InsumosModel> CriarAsync(InsumosModel entity);
        Task<InsumosModel> AtualizarAsync(InsumosModel entity);
        Task<bool> DeletarAsync(int id); // A implementação fará soft delete
    }
}