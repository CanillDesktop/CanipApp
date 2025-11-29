using Shared.DTOs.Medicamentos;

namespace Backend.Services.Interfaces
{
    public interface IMedicamentosService : IService<MedicamentoCadastroDTO, MedicamentoLeituraDTO>
    {
        Task<IEnumerable<MedicamentoLeituraDTO>> BuscarTodosAsync(MedicamentosFiltroDTO filtro);
    }
}