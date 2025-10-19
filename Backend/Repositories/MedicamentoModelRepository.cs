
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

        public async Task<IEnumerable<MedicamentosModel>> Get()
        {
            var  medicamentosRepository = await _context.Medicamentos.ToListAsync();

            return medicamentosRepository is null ? throw new InvalidOperationException("Medicamentos é null") : medicamentosRepository;
        }

        public async Task<MedicamentosModel> GetMedicamento(int id)
        {

            var medicamentosRepository = await _context.Medicamentos.FirstOrDefaultAsync(p =>p.CodigoId == id);
            return medicamentosRepository is null ? throw new InvalidOperationException("Medicamentos é null") : medicamentosRepository;
        }
        public async Task<MedicamentosModel> CreateMedicamento(MedicamentosModel Medicamento)
        {
            if (Medicamento is null)
            {
                throw new ArgumentNullException(nameof(Medicamento));
            }

            await _context.Medicamentos.AddAsync(Medicamento);
           await  _context.SaveChangesAsync();
            return Medicamento;

        }


        public async Task<MedicamentosModel> UpdateMedicamento(MedicamentosModel Medicamento)
        {
            if (Medicamento is null)
            {
                throw new ArgumentException(null, nameof(Medicamento));
            }
            _context.Entry(Medicamento).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Medicamento;
        }

        public async Task<MedicamentosModel> DeleteMedicamento(int id)
        {
            var medicamentosRepository = await _context.Medicamentos.FindAsync(id);

            if (medicamentosRepository is null)
            {
                throw new ArgumentException(nameof(medicamentosRepository));
            }

            _context.Medicamentos.Remove(medicamentosRepository);
            await _context.SaveChangesAsync();
            return medicamentosRepository;
        }

       
     
    }
}
