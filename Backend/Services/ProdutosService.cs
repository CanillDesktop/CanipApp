using Backend.Exceptions;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Shared.DTOs.Produtos;
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

        public async Task<IEnumerable<ProdutosLeituraDTO>> BuscarTodosAsync() => (await _repository.GetAsync()).Select(p => (ProdutosLeituraDTO)p);

        public async Task<ProdutosLeituraDTO?> BuscarPorIdAsync(int id) => (await _repository.GetByIdAsync(id))!;

        public async Task<ProdutosLeituraDTO?> CriarAsync(ProdutosCadastroDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CodProduto)
                || string.IsNullOrWhiteSpace(dto.DescricaoSimples)
                || !Enum.IsDefined(typeof(UnidadeEnum), (int)dto.Unidade)
                || !Enum.IsDefined(typeof(CategoriaEnum), (int)dto.Categoria)
                || string.IsNullOrWhiteSpace(dto.Lote))
            {
                throw new ModelIncompletaException("Um ou mais campos obrigatórios não foram preenchidos");
            }

            return await _repository.CreateAsync(dto);
        }

        public async Task<ProdutosLeituraDTO?> AtualizarAsync(ProdutosCadastroDTO dto) => (await _repository.UpdateAsync(dto))!;

        public async Task<bool> DeletarAsync(int id) => await _repository.DeleteAsync(id);

        public async Task<IEnumerable<ProdutosLeituraDTO>> BuscarTodosAsync(ProdutosFiltroDTO filtro) => (await _repository.GetAsync(filtro)).Select(p => (ProdutosLeituraDTO)p);
    }
}
