using Backend.Exceptions;
using Backend.Repositories;
using Shared.DTOs.Estoque;

namespace Backend.Services
{
    public class EstoqueItemService
    {
        private readonly EstoqueItemRepository _repository;

        public EstoqueItemService(EstoqueItemRepository repository)
        {
            _repository = repository;
        }

        public async Task<ItemEstoqueDTO?> BuscarPorIdAsync(int id) => (await _repository.GetByIdAsync(id))!;

        public async Task<ItemEstoqueDTO?> CriarAsync(ItemEstoqueDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CodItem))
            {
                throw new ModelIncompletaException("Um ou mais campos obrigatórios não foram preenchidos");
            }

            return await _repository.CreateAsync(dto);
        }

        public async Task<ItemEstoqueDTO?> AtualizarAsync(ItemEstoqueDTO dto) => (await _repository.UpdateAsync(dto))!;

        public async Task<bool> DeletarAsync(string lote) => await _repository.DeleteAsync(lote);
    }
}
