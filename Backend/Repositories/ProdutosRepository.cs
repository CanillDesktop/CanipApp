using Backend.Context;
using Backend.Models.Produtos;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ProdutosRepository : IProdutosRepository
    {
        private readonly CanilAppDbContext _context;

        public ProdutosRepository(CanilAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProdutosModel>> GetAsync()
        {
            var produtos = await _context.Produtos.ToListAsync();
            return produtos;
        }

        public async Task<ProdutosModel?> GetByIdAsync(string id)
        {
            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.IdProduto == id);

            ArgumentNullException.ThrowIfNull(produto);

            return produto;
        }

        public async Task<ProdutosModel> CreateAsync(ProdutosModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            await _context.Produtos.AddAsync(model);
            await _context.SaveChangesAsync();

            return model;
        }

        public async Task<ProdutosModel?> UpdateAsync(ProdutosModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var produto = await _context.Produtos.FindAsync(id);

            ArgumentNullException.ThrowIfNull(produto);

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
