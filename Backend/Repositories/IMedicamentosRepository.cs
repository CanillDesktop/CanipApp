using Backend.Models.Medicamentos;

namespace Backend.Repositories
{
    public interface IMedicamentosRepository
    {
        IEnumerable<MedicamentosModel> GetMedicamentosModel();
       MedicamentosModel GetMedicamentosModel(int id);
        
        MedicamentosModel CreateMedicamentosModel(MedicamentosModel Medicamento);

        MedicamentosModel UpdateMedicamentosModel(MedicamentosModel Medicamento);

        MedicamentosModel DeleteMedicamentosModel(int id);
    }
}
