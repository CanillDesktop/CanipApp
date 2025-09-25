using Backend.Models.Medicamentos;

namespace Backend.Repositories
{
    public interface IMedicamentosRepository
    {
        IEnumerable<MedicamentosModel> Get();
       MedicamentosModel GetMedicamento(int id);
        
        MedicamentosModel CreateMedicamento(MedicamentosModel Medicamento);

        MedicamentosModel UpdateMedicamento(MedicamentosModel Medicamento);

        MedicamentosModel DeleteMedicamento(int id);
    }
}
