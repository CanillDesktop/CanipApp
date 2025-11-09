using Backend.Context;
using Backend.Models.Medicamentos;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Backend.Repositories
{
    public class MedicamentosRepository : IMedicamentosRepository
    {
        private readonly CanilAppDbContext _context;

        public MedicamentosRepository(CanilAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MedicamentosModel>> BuscarTodosAsync(Expression<Func<MedicamentosModel, bool>>? filter = null)
        {
            IQueryable<MedicamentosModel> query = _context.Medicamentos;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<MedicamentosModel?> BuscarPorIdAsync(int id)
        {
            return await _context.Medicamentos.AsNoTracking()
                                 .FirstOrDefaultAsync(m => m.CodigoId == id);
        }

        public async Task<MedicamentosModel> CriarAsync(MedicamentosModel model)
        {
            await _context.Medicamentos.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<MedicamentosModel?> AtualizarAsync(MedicamentosModel model)
        {
            _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> DeletarAsync(int id)
        {
            // AVISO: Isto é um "Hard Delete".
            // Para "Soft Delete", você deve implementar essa lógica no SERVIÇO,
            // chamando o método AtualizarAsync() deste repositório.

            var model = await _context.Medicamentos.FindAsync(id);
            if (model == null)
            {
                return false;
            }

            _context.Medicamentos.Remove(model);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}