using Backend.Models.Medicamentos;
using Shared.DTOs;

namespace Backend.Repositories
{
    public interface IInsumosRepository
    {
     
        
            Task<IEnumerable<InsumosModel>> Get();
            Task<InsumosModel> GetInsumo(int id);

            Task<InsumosModel> CreateInsumo(InsumosModel Insumos);

            Task<InsumosModel> UpdateInsumo(InsumosModel Insumos);

            Task<InsumosModel> DeleteInsumo(int id);

        
    }

}

