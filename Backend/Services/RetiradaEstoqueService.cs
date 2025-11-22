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

            return await _repository.CreateAsync(dto);
        }
    }
}
