using Backend.Context;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs.Estoque;

namespace Backend.Repositories
{
    public class RetiradaEstoqueRepository
    {
        private readonly CanilAppDbContext _context;

        public RetiradaEstoqueRepository(CanilAppDbContext context)
        {
            _context = context;
        }

        public async Task<RetiradaEstoqueDTO?> CreateAsync(RetiradaEstoqueDTO dto)
        {
            RetiradaEstoqueModel model = dto;
            model.DataAtualizacao = DateTime.UtcNow; // ✅ Atualizar timestamp

            await _context.RetiradaEstoque.AddAsync(model);
            await _context.SaveChangesAsync();

            return model;
        }

        // ✅ ADICIONAR método de atualização
        public async Task<RetiradaEstoqueDTO?> UpdateAsync(RetiradaEstoqueDTO dto)
        {
            var existing = await _context.RetiradaEstoque
                .FirstOrDefaultAsync(r => r.IdRetirada == dto.IdRetirada);

            if (existing == null) return null;

            existing.CodItem = dto.CodItem;
            existing.NomeItem = dto.NomeItem;
            existing.Quantidade = dto.Quantidade;
            existing.Lote = dto.Lote;
            existing.De = dto.De;
            existing.Para = dto.Para;
            existing.DataAtualizacao = DateTime.UtcNow; // ✅ Atualizar timestamp

            await _context.SaveChangesAsync();
            return existing;
        }

        // ✅ ADICIONAR soft delete
        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.RetiradaEstoque
                .FirstOrDefaultAsync(r => r.IdRetirada == id);

            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.DataAtualizacao = DateTime.UtcNow; // ✅ Atualizar timestamp

            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ ADICIONAR método de listagem (filtrar IsDeleted)
        public async Task<IEnumerable<RetiradaEstoqueDTO>> GetAsync()
        {
            var models = await _context.RetiradaEstoque
                .Where(r => !r.IsDeleted) // ✅ Excluir soft-deleted
                .ToListAsync();

            return models.Select(m => (RetiradaEstoqueDTO)m);
        }

        public async Task<RetiradaEstoqueDTO?> GetByIdAsync(int id)
        {
            var model = await _context.RetiradaEstoque
                .FirstOrDefaultAsync(r => r.IdRetirada == id && !r.IsDeleted);

            return model;
        }
    }
}