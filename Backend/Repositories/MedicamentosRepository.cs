
using Backend.Context;
using Backend.Models.Medicamentos;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class MedicamentosRepository : IMedicamentosRepository
    {

        private readonly CanilAppDbContext _context;

        public MedicamentosRepository(CanilAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MedicamentosModel>> GetAsync()
        {
            var medicamentosRepository = await _context.Medicamentos.ToListAsync();

            return medicamentosRepository is null ? throw new InvalidOperationException("Medicamentos é null") : medicamentosRepository;
        }

        public async Task<MedicamentosModel?> GetByIdAsync(int id)
        {

            var medicamentosRepository = await _context.Medicamentos.FirstOrDefaultAsync(p => p.CodigoId == id);
            return medicamentosRepository is null ? throw new InvalidOperationException("Medicamentos é null") : medicamentosRepository;
        }
        public async Task<MedicamentosModel> CreateAsync(MedicamentosModel Medicamento)
        {
            if (Medicamento is null)
            {
                throw new ArgumentNullException(nameof(Medicamento));
            }

            await _context.Medicamentos.AddAsync(Medicamento);
            await _context.SaveChangesAsync();
            return Medicamento;

        }


        public async Task<MedicamentosModel?> UpdateAsync(MedicamentosModel Medicamento)
        {
            if (Medicamento is null)
            {
                throw new ArgumentException(null, nameof(Medicamento));
            }
            _context.Entry(Medicamento).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Medicamento;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var medicamentosRepository = await _context.Medicamentos.FindAsync(id);

            if (medicamentosRepository is null)
            {
                throw new ArgumentException(nameof(medicamentosRepository));
            }

            _context.Medicamentos.Remove(medicamentosRepository);
            await _context.SaveChangesAsync();
            return true;
        }



    }
}
