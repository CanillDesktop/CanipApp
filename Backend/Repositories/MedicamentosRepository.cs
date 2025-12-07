using Backend.Context;
using Backend.Models.Medicamentos;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs.Medicamentos;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Repositories
{
    public class MedicamentosRepository : IMedicamentosRepository
    {
        private readonly CanilAppDbContext _context;

        public MedicamentosRepository(CanilAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MedicamentosModel>> GetAsync()
        {
            var medicamentosRepository = await _context.Medicamentos
                .Include(m => m.ItemNivelEstoque)
                .Include(m => m.ItensEstoque)
                .Where(m => m.IsDeleted == false) // ✅ FILTRO DELETADOS
                .ToListAsync();

           
            return medicamentosRepository;
        }

        public async Task<MedicamentosModel?> GetByIdAsync(int id)
        {
            var medicamentosRepository = await _context.Medicamentos
                .Include(m => m.ItensEstoque)
                .Include(m => m.ItemNivelEstoque)
                .Where(m => m.IsDeleted == false) // ✅ FILTRO DELETADOS
                .FirstOrDefaultAsync(m => m.IdItem == id);

            
            return medicamentosRepository;
        }

        public async Task<MedicamentosModel> CreateAsync(MedicamentosModel Medicamento)
        {
            if (Medicamento is null)
            {
                throw new ArgumentNullException(nameof(Medicamento));
            }

            await _context.Medicamentos.AddAsync(Medicamento);
            await _context.SaveChangesAsync();
            return Medicamento;
        }

        public async Task<MedicamentosModel?> UpdateAsync(MedicamentosModel Medicamento)
        {
            if (Medicamento is null)
            {
                throw new ArgumentException(null, nameof(Medicamento));
            }
            _context.Entry(Medicamento).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Medicamento;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var medicamento = await _context.Medicamentos.FindAsync(id);

            if (medicamento is null)
            {
                // Se já não existe, retorna false ou lança erro (conforme sua preferência)
                // Mantendo sua lógica de exceção se preferir, ou retornando false.
                return false;
            }

            // ════════════════════════════════════════════════════════════
            // ✅ SOFT DELETE (Essencial para o Sync funcionar)
            // ════════════════════════════════════════════════════════════

            // 1. Marca como deletado
            medicamento.IsDeleted = true;

            // 2. Atualiza a data para vencer a versão da Nuvem
            medicamento.DataAtualizacao = DateTime.UtcNow;

            // 3. Atualiza em vez de remover
            _context.Medicamentos.Update(medicamento);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<MedicamentosModel>> GetAsync(MedicamentosFiltroDTO filtro)
        {
            ArgumentNullException.ThrowIfNull(filtro);

            var query = _context.Medicamentos
                .Include(m => m.ItensEstoque)
                .Include(m => m.ItemNivelEstoque)
                .Where(m => m.IsDeleted == false) // ✅ FILTRO DELETADOS
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.CodMedicamento))
                query = query.Where(p => p.CodMedicamento!.Contains(filtro.CodMedicamento));

            if (!string.IsNullOrWhiteSpace(filtro.NomeComercial))
                query = query.Where(p => p.NomeComercial!.Contains(filtro.NomeComercial));

            if (!string.IsNullOrWhiteSpace(filtro.Formula))
                query = query.Where(p => p.Formula!.Contains(filtro.Formula));

            if (!string.IsNullOrWhiteSpace(filtro.DescricaoMedicamento))
                query = query.Where(p => p.DescricaoMedicamento!.Contains(filtro.DescricaoMedicamento));

            if (!string.IsNullOrWhiteSpace(filtro.NFe))
                query = query.Where(p => p.ItensEstoque!.Any(p => p.NFe.Contains(filtro.NFe)));

            if (Enum.IsDefined(typeof(PrioridadeEnum), filtro.Prioridade))
                query = query.Where(p => p.Prioridade == (PrioridadeEnum)filtro.Prioridade);

            if (Enum.IsDefined(typeof(PublicoAlvoMedicamentoEnum), filtro.PublicoAlvo))
                query = query.Where(p => p.PublicoAlvo == (PublicoAlvoMedicamentoEnum)filtro.PublicoAlvo);

            if (filtro.DataEntrega != null)
                query = query.Where(p => p.ItensEstoque!.Any(e => e.DataEntrega == filtro.DataEntrega));

            if (filtro.DataValidade != null)
                query = query.Where(p => p.ItensEstoque!.Any(e => e.DataValidade == filtro.DataValidade));

            var medicamentos = await query.ToListAsync();
            return medicamentos;
        }
    }
}