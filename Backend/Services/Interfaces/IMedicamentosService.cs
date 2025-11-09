using Shared.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IMedicamentosService : IService<MedicamentoDTO, int>
    {
        Task<IEnumerable<MedicamentoDTO>> BuscarTodosAsync();
        Task<MedicamentoDTO?> BuscarPorIdAsync(int id);
        Task<MedicamentoDTO> CriarAsync(MedicamentoDTO medicamentoDto);
        Task<MedicamentoDTO?> AtualizarAsync(MedicamentoDTO medicamentoDto);
        Task<bool> DeletarAsync(int id);
    }
}