using Backend.Models.Insumos;
using Shared.DTOs.Insumos;

namespace Backend.Repositories.Interfaces
{
    public interface IInsumosRepository : IRepository<InsumosModel>
    {
        Task<IEnumerable<InsumosModel>> GetAsync(InsumosFiltroDTO filtro);
    }
}

