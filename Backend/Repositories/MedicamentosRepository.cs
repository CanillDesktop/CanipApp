
using Backend.Context;
using Backend.Models.Medicamentos;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs.Medicamentos;
using Shared.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                .ToListAsync();

            return medicamentosRepository is null ? throw new InvalidOperationException("Medicamentos é null") : medicamentosRepository;
            }

        public async Task<MedicamentosModel?> GetByIdAsync(int id)
        {
            var medicamentosRepository = await _context.Medicamentos
                .Include(m => m.ItensEstoque)
                .Include(m => m.ItemNivelEstoque)
                .FirstOrDefaultAsync(m => m.IdItem == id);

            return medicamentosRepository is null ? throw new InvalidOperationException("Medicamentos é null") : medicamentosRepository;
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
            var medicamentosRepository = await _context.Medicamentos.FindAsync(id);

            if (medicamentosRepository is null)
            {
                throw new ArgumentException(nameof(medicamentosRepository));
            }

            _context.Medicamentos.Remove(medicamentosRepository);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MedicamentosModel>> GetAsync(MedicamentosFiltroDTO filtro)
        {
            ArgumentNullException.ThrowIfNull(filtro);

            var query = _context.Medicamentos
                .Include(m => m.ItensEstoque)
                .Include(m => m.ItemNivelEstoque)
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

            var produtos = await query.ToListAsync();
            return produtos;
        }

    }
}