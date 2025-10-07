using Backend.Context;
using Backend.Models.Medicamentos;
using Microsoft.EntityFrameworkCore;
using Backend.Repositories;

namespace Backend.Repositories
{

        
        public class IInsumosModelRepository : IInsumosRepository
        {

            private readonly CanilAppDbContext _context;

            public IInsumosModelRepository(CanilAppDbContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<InsumosModel>> Get()
            {
                var InsumosRepository = await _context.Insumos.ToListAsync();

                return InsumosRepository is null ? throw new InvalidOperationException("Medicamentos é null") : InsumosRepository;
            }

            public async Task<InsumosModel> GetInsumo(int id)
            {

                var InsumosRepository = await _context.Insumos.FirstOrDefaultAsync(p => p.CodigoId == id);
                return InsumosRepository is null ? throw new InvalidOperationException("Medicamentos é null") : InsumosRepository;
            }
            public async Task<InsumosModel> CreateInsumo(InsumosModel Insumo)
            {
                if (Insumo is null)
                {
                    throw new ArgumentNullException(nameof(Insumo));
                }

                await _context.Insumos.AddAsync(Insumo);
                await _context.SaveChangesAsync();
                return Insumo;

            }


            public async Task<InsumosModel> UpdateInsumo(InsumosModel Insumo)
            {
                if (Insumo is null)
                {
                    throw new ArgumentException(null, nameof(Insumo));
                }
                _context.Entry(Insumo).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Insumo;
            }

            public async Task<InsumosModel> DeleteInsumo(int id)
            {
                var Insumosrepository = await _context.Insumos.FindAsync(id);

                if (Insumosrepository is null)
                {
                    throw new ArgumentException(nameof(Insumosrepository));
                }

                _context.Insumos.Remove(Insumosrepository);
                await _context.SaveChangesAsync();
                return Insumosrepository;
            }



        }
    }



