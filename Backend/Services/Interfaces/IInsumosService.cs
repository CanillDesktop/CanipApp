using Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services.Interfaces
{
    public interface IInsumosService
    {
        Task<IEnumerable<InsumosDTO>> BuscarTodosAsync();
        Task<InsumosDTO?> BuscarPorIdAsync(int id);
        Task<InsumosDTO> CriarAsync(InsumosDTO dto);
        Task<InsumosDTO?> AtualizarAsync(InsumosDTO dto);
        Task<bool> DeletarAsync(int id);
    }
}