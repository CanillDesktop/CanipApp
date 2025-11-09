using Backend.Context;
using Backend.Exceptions;
using Backend.Models.Produtos;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using Shared.Enums;
using System.Linq;
using System.Linq.Expressions;

namespace Backend.Repositories
{
    public class ProdutosRepository : IProdutosRepository
    {
        private readonly CanilAppDbContext _context;

        public ProdutosRepository(CanilAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Produtos>> BuscarTodosAsync(Expression<Func<Produtos, bool>>? filter = null)
        {
            IQueryable<Produtos> query = _context.Produtos;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<Produtos?> BuscarPorIdAsync(string id)
        {
             return await _context.Produtos.FindAsync(id);
        }

        public async Task<Produtos> CriarAsync(Produtos model)
        {
            await _context.Produtos.AddAsync(model);
            await _context.SaveChangesAsync();
            return model;
        }


        public async Task<Produtos?> AtualizarAsync(Produtos model)
        {
            _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> DeletarAsync(string id)
        {
            // AVISO: Isto é um "Hard Delete".
            // Para "Soft Delete", você deve implementar essa lógica no SERVIÇO,
            // chamando o método AtualizarAsync() deste repositório.

            var model = await _context.Produtos.FindAsync(id);
            if (model == null)
            {
                return false;
            }

            _context.Produtos.Remove(model);
            await _context.SaveChangesAsync();
            return true;
        }



    }
}
