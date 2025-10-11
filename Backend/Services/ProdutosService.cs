using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Shared.DTOs;

namespace Backend.Services
{
    public class ProdutosService : IProdutosService
    {
        private readonly IProdutosRepository _repository;

        public ProdutosService(IProdutosRepository repository) 
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProdutosDTO>> BuscarTodosAsync() => (await _repository.GetAsync()).Select(p => (ProdutosDTO)p);

        public async Task<ProdutosDTO?> BuscarPorIdAsync(string id) => (await _repository.GetByIdAsync(id))!;

        public async Task<ProdutosDTO?> CriarAsync(ProdutosDTO dto) => await _repository.CreateAsync(dto);

        public async Task<ProdutosDTO?> AtualizarAsync(ProdutosDTO dto) => (await _repository.UpdateAsync(dto))!;

        public async Task<bool> DeletarAsync(string id) => await _repository.DeleteAsync(id);
    }
}
