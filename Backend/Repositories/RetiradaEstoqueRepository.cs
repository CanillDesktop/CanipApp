using Backend.Context;
using Backend.Models;

namespace Backend.Repositories
{
    public class RetiradaEstoqueRepository
    {
        private readonly CanilAppDbContext _context;

        public RetiradaEstoqueRepository(CanilAppDbContext context)
        {
            _context = context;
        }

        public async Task<RetiradaEstoqueModel> CreateAsync(RetiradaEstoqueModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            _context.RetiradaEstoque.Add(model);
            await _context.SaveChangesAsync();

            return model;
        }
    }
}
