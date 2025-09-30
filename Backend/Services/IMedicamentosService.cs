using Shared.DTOs;

namespace Backend.Services
{
    public interface IMedicamentosService
    {
        Task<IEnumerable<MedicamentoDTO>> GetAllMedicamentos();
        Task<MedicamentoDTO?> GetMedById(int id);
        Task<MedicamentoDTO> CreateMedicamento(MedicamentoDTO medicamentoDto);
        Task<MedicamentoDTO?> UpdateMedicamento(MedicamentoDTO medicamentoDto);
        Task<bool> DeleteMedicamento(int id);
    }
}