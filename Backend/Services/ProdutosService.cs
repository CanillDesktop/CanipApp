using Backend.Exceptions;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Shared.DTOs;
using Shared.Enums;

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

        public async Task<ProdutosDTO?> CriarAsync(ProdutosDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.IdProduto)
                || string.IsNullOrWhiteSpace(dto.DescricaoSimples)
                || dto.DataEntrega == null
                || !Enum.IsDefined(typeof(UnidadeEnum), (int)dto.Unidade)
                || !Enum.IsDefined(typeof(CategoriaEnum), (int)dto.Categoria))
            {
                throw new ModelIncompletaException("Um ou mais campos obrigatórios não foram preenchidos");
            }

            return await _repository.CreateAsync(dto);
        }

        public async Task<ProdutosDTO?> AtualizarAsync(ProdutosDTO dto) => (await _repository.UpdateAsync(dto))!;

        public async Task<bool> DeletarAsync(string id) => await _repository.DeleteAsync(id);

        public async Task<IEnumerable<ProdutosDTO>> BuscarTodosAsync(ProdutosFiltroDTO filtro) => (await _repository.GetAsync(filtro)).Select(p => (ProdutosDTO)p);
    }
}
