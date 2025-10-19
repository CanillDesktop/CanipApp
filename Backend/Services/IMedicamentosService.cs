using Shared.DTOs;

namespace Backend.Services
{
    public interface IMedicamentosService
    {
        Task<IEnumerable<MedicamentoDTO>> RetornarMedicamentos();
        Task<MedicamentoDTO?> RetornarMedicamentoId(int id);
        Task<MedicamentoDTO>CriarMedicamento(MedicamentoDTO medicamentoDto);
        Task<MedicamentoDTO?> AtualizarMedicamento(MedicamentoDTO medicamentoDto);
        Task<bool> DeletarMedicamento(int id);
    }
}