using Backend.Context;
using Backend.Exceptions;
using Backend.Models.Produtos;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using Shared.Enums;

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

            model.DataHoraInsercaoRegistro = DateTime.Now;

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

        public async Task<IEnumerable<ProdutosModel>> GetAsync(ProdutosFiltroDTO filtro)
        {
            ArgumentNullException.ThrowIfNull(filtro);

            var query = _context.Produtos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.IdProduto))
                query = query.Where(p => p.IdProduto!.Contains(filtro.IdProduto));

            if (!string.IsNullOrWhiteSpace(filtro.DescricaoSimples))
                query = query.Where(p => p.DescricaoSimples!.Contains(filtro.DescricaoSimples));

            if (!string.IsNullOrWhiteSpace(filtro.NFe))
                query = query.Where(p => p.NFe!.Contains(filtro.NFe));

            if (Enum.IsDefined(typeof(CategoriaEnum), filtro.Categoria))
                query = query.Where(p => p.Categoria == (CategoriaEnum)filtro.Categoria);

            if (filtro.DataEntrega != null)
                query = query.Where(p => p.DataEntrega == filtro.DataEntrega);

            if (filtro.DataValidade != null)
                query = query.Where(p => p.Validade == filtro.DataValidade);

            var produtos = await query.ToListAsync();
            return produtos;
        }

    }
}
