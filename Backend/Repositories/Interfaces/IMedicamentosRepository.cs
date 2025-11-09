using Backend.Models.Medicamentos; // MUDANÇA: Usar o namespace do Modelo
using System.Collections.Generic;
using System.Linq.Expressions; // Para o filtro (soft delete)
using System.Threading.Tasks;

public interface IMedicamentosRepository
{
   
    Task<IEnumerable<MedicamentosModel>> BuscarTodosAsync(Expression<Func<MedicamentosModel, bool>>? filter = null);

    Task<MedicamentosModel?> BuscarPorIdAsync(int id);

    Task<MedicamentosModel> CriarAsync(MedicamentosModel model);

    Task<MedicamentosModel?> AtualizarAsync(MedicamentosModel model);

    Task<bool> DeletarAsync(int id);


}