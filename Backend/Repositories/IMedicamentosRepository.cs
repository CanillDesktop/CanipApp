using Backend.Models.Medicamentos;
using Shared.DTOs;

namespace Backend.Repositories
{
    public interface IMedicamentosRepository
    {
        Task<IEnumerable<MedicamentosModel>> Get();
       Task<MedicamentosModel> GetMedicamento(int id);
        
       Task< MedicamentosModel>  CreateMedicamento(MedicamentosModel Medicamento);

        Task<MedicamentosModel> UpdateMedicamento(MedicamentosModel Medicamento);

        Task <MedicamentosModel> DeleteMedicamento(int id);
      
    }
}
