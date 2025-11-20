using Shared.DTOs.Insumos;

namespace Backend.Services.Interfaces
{
    public interface IInsumosService : IService<InsumosCadastroDTO, InsumosLeituraDTO>
    {
        Task<IEnumerable<InsumosLeituraDTO>> BuscarTodosAsync(InsumosFiltroDTO filtro);
    }
}

