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

        public async Task<bool> DeleteAsync(string lote)
        {
            var item = await _context.ItensEstoque.FirstOrDefaultAsync(x => x.Lote == lote);

            ArgumentNullException.ThrowIfNull(item);

            _context.ItensEstoque.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
