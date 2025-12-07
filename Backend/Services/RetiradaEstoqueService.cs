using Backend.Exceptions;
using Backend.Repositories;
using Shared.DTOs.Estoque;

namespace Backend.Services
{
    public class RetiradaEstoqueService
    {
        private readonly RetiradaEstoqueRepository _repository;

        public RetiradaEstoqueService(RetiradaEstoqueRepository repository)
        {
            _repository = repository;
        }

        public async Task<RetiradaEstoqueDTO?> CriarAsync(RetiradaEstoqueDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CodItem)
                || string.IsNullOrWhiteSpace(dto.NomeItem)
                || string.IsNullOrWhiteSpace(dto.De)
                || string.IsNullOrWhiteSpace(dto.Para))
            {
                throw new ModelIncompletaException("Um ou mais campos obrigatórios não foram preenchidos");
            }

            // ✅ Garantir que DataHoraInsercaoRegistro está preenchida
            if (dto.DataHoraInsercaoRegistro == DateTime.MinValue)
            {
                dto.DataHoraInsercaoRegistro = DateTime.UtcNow;
            }

            return await _repository.CreateAsync(dto);
        }

        // ✅ ADICIONAR método de atualização (se necessário)
        public async Task<RetiradaEstoqueDTO?> AtualizarAsync(RetiradaEstoqueDTO dto)
        {
            return await _repository.UpdateAsync(dto);
        }

        // ✅ ADICIONAR soft delete
        public async Task<bool> DeletarAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}