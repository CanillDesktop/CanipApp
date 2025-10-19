using Shared.DTOs;

namespace Backend.Services
{
    public interface IInsumosService
    {
       
      
            Task<IEnumerable<InsumosDTO>> RetornarInsumo();
            Task<InsumosDTO?> RetornarInsumoId(int id);
            Task<InsumosDTO> CriarInsumo(InsumosDTO insumoDto);
            Task<InsumosDTO?> AtualizarMedicamento(InsumosDTO insumoDto);
            Task<bool> DeletarInsumo(int id);
        
    }
}

