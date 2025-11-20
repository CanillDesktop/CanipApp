using Shared.DTOs.Produtos;

namespace Backend.Services.Interfaces
{
    public interface IProdutosService : IService<ProdutosCadastroDTO, ProdutosLeituraDTO>
    {
        Task<IEnumerable<ProdutosLeituraDTO>> BuscarTodosAsync(ProdutosFiltroDTO filtro);
    }
}
