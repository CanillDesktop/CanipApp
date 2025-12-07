using Backend.Context;
using Backend.Models.Insumos;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs.Insumos;
using Shared.Enums;

namespace Backend.Repositories
{
    public class IInsumosModelRepository : IInsumosRepository
    {

        private readonly CanilAppDbContext _context;
        private readonly DbSet<InsumosModel> _dbSet;

        public IInsumosModelRepository(CanilAppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<InsumosModel>();
        }

        public async Task<IEnumerable<InsumosModel>> GetAsync()
        {
            var insumosRepository = await _context.Insumos
                .Include(i => i.ItensEstoque)
                .Include(i => i.ItemNivelEstoque)
                .ToListAsync();

            return insumosRepository is null ? throw new InvalidOperationException("Insumos é null") : insumosRepository;
        }

        public async Task<InsumosModel?> GetByIdAsync(int id)
        {

            var insumosRepository = await _context.Insumos
                .Include(i => i.ItensEstoque)
                .Include(i => i.ItemNivelEstoque)
                .FirstOrDefaultAsync(i => i.IdItem == id);

            return insumosRepository is null ? throw new InvalidOperationException("Insumos é null") : insumosRepository;
        }

        public async Task<InsumosModel> CreateAsync(InsumosModel insumo)
        {
            if (insumo is null)
            {
                throw new ArgumentNullException(nameof(insumo));
            }

            await _context.Insumos.AddAsync(insumo);
            await _context.SaveChangesAsync();
            return insumo;

        }


        public async Task<InsumosModel> UpdateAsync(InsumosModel Insumo)
        {
            if (Insumo is null)
            {
                throw new ArgumentException(null, nameof(Insumo));
            }
            _context.Entry(Insumo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Insumo;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var Insumosrepository = await _context.Insumos.FindAsync(id);

            if (Insumosrepository is null)
            {
                throw new ArgumentException(nameof(Insumosrepository));
            }

            _context.Insumos.Remove(Insumosrepository);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<InsumosModel>> GetAsync(InsumosFiltroDTO filtro)
        {
            ArgumentNullException.ThrowIfNull(filtro);

            var query = _context.Insumos
                .Include(i => i.ItensEstoque)
                .Include(i => i.ItemNivelEstoque)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.CodInsumo))
                query = query.Where(p => p.CodInsumo!.Contains(filtro.CodInsumo));

            if (!string.IsNullOrWhiteSpace(filtro.DescricaoSimplificada))
                query = query.Where(p => p.DescricaoSimplificada!.Contains(filtro.DescricaoSimplificada));

            if (!string.IsNullOrWhiteSpace(filtro.NFe))
                query = query.Where(p => p.ItensEstoque!.Any(p => p.NFe.Contains(filtro.NFe)));

        

            if (filtro.DataEntrega != null)
                query = query.Where(p => p.ItensEstoque!.Any(e => e.DataEntrega == filtro.DataEntrega));


            if (filtro.DataValidade != null)
                query = query.Where(p => p.ItensEstoque!.Any(e => e.DataValidade == filtro.DataValidade));

            var insumos = await query.ToListAsync();
            return insumos;
        }
    }
}



