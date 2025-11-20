using Backend.Context;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class EstoqueItemRepository
    {
        private readonly CanilAppDbContext _context;

        public EstoqueItemRepository(CanilAppDbContext context)
        {
            _context = context;
        }

        public async Task<ItemEstoqueModel?> GetByIdAsync(int id)
        {
            var itemEstoque = await _context.ItensEstoque.FirstOrDefaultAsync(i => i.IdItem == id);

            ArgumentNullException.ThrowIfNull(itemEstoque);

            return itemEstoque;
        }

        public async Task<ItemEstoqueModel> CreateAsync(ItemEstoqueModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            _context.ItensEstoque.Add(model);
            await _context.SaveChangesAsync();

            return model;
        }


        public async Task<ItemEstoqueModel?> UpdateAsync(ItemEstoqueModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);

            ArgumentNullException.ThrowIfNull(produto);

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
