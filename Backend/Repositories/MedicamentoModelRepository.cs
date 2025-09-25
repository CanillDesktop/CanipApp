
using Backend.Context;
using Backend.Models.Medicamentos;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class MedicamentoModelRepository : IMedicamentosRepository
    {

        private readonly CanilAppDbContext _context;

        public MedicamentoModelRepository(CanilAppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<MedicamentosModel> GetMedicamentosModel()
        {
          var  medicamentosRepository =  _context.Medicamentos.ToList();

            return medicamentosRepository is null ? throw new InvalidOperationException("Medicamentos é null") : medicamentosRepository;
        }

        public MedicamentosModel GetMedicamentosModel(int id)
        {

            var medicamentosRepository = _context.Medicamentos.FirstOrDefault(p =>p.CodigoId == id);
            return medicamentosRepository is null ? throw new InvalidOperationException("Medicamentos é null") : medicamentosRepository;
        }
        public MedicamentosModel CreateMedicamentosModel(MedicamentosModel Medicamento)
        {
            if (Medicamento is null)
            {
                throw new ArgumentException(nameof(Medicamento));
            }

            _context.Medicamentos.Add(Medicamento);
            _context.SaveChanges();
            return Medicamento;

        }


        public MedicamentosModel UpdateMedicamentosModel(MedicamentosModel Medicamento)
        {
            if (Medicamento is null)
            {
                throw new ArgumentException(nameof(Medicamento));
            }
            _context.Entry(Medicamento).State = EntityState.Modified;
            _context.SaveChanges();
            return Medicamento;
        }

        public MedicamentosModel DeleteMedicamentosModel(int id)
        {
            var medicamentosRepository = _context.Medicamentos.Find(id);

            if (medicamentosRepository is null)
            {
                throw new ArgumentException(nameof(medicamentosRepository));
            }

            _context.Medicamentos.Remove(medicamentosRepository);
            _context.SaveChanges();
            return medicamentosRepository;
        }

       
     
    }
}
