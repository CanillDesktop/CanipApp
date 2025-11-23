using Backend.Models.Medicamentos;
using Shared.DTOs.Medicamentos;

namespace Backend.Repositories.Interfaces
{
    public interface IMedicamentosRepository : IRepository<MedicamentosModel>
    {
        Task<IEnumerable<MedicamentosModel>> GetAsync(MedicamentosFiltroDTO filtro);
    }
}