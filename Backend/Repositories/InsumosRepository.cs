using Backend.Context;
using Backend.Models.Insumos;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Backend.Repositories
{
    // Esta implementação assume que você NÃO tem uma classe Repository<T> genérica.
    // Se tiver, apenas herde dela:
    // public class InsumosRepository : Repository<InsumosModel, int>, IInsumosRepository

    public class InsumosRepository : IInsumosRepository
    {
        private readonly CanilAppDbContext _context;
        private readonly DbSet<InsumosModel> _dbSet;

        public InsumosRepository(CanilAppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<InsumosModel>();
        }

        public async Task<IEnumerable<InsumosModel>> BuscarTodosAsync(Expression<Func<InsumosModel, bool>>? predicate = null)
        {
            var query = _dbSet.AsQueryable();
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<InsumosModel?> BuscarPorIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<InsumosModel> CriarAsync(InsumosModel entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<InsumosModel> AtualizarAsync(InsumosModel entity)
        {
            
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            entry.State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeletarAsync(int id)
        {
            // A lógica de soft delete será tratada no SERVICE.
            // O repositório apenas executa o que o service manda.
            // Seguindo o padrão de Produtos, o service buscará,
            // marcará IsDeleted e chamará AtualizarAsync.

            // Se o padrão fosse hard delete no repositório:
            // var entity = await BuscarPorIdAsync(id);
            // if (entity == null) return false;
            // _dbSet.Remove(entity);
            // await _context.SaveChangesAsync();
            // return true;

            // No seu padrão (ProdutosService), DeletarAsync do repositório
            // nem é chamado. O service chama BuscarPorIdAsync e AtualizarAsync.
            // Vou manter este método por consistência, mas o service fará soft-delete
            // via AtualizarAsync.

            var entity = await BuscarPorIdAsync(id);
            if (entity == null) return false;

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
            // NOTA: O InsumosService NÃO usará este método se seguir o padrão 
            // de soft-delete do ProdutosService.
        }
    }
}