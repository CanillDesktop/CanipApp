using Backend.Models.Produtos;
using Shared.DTOs;

namespace Backend.Services
{
    public class ProdutosService
    {
        private readonly List<ProdutosModel> _produtos = [];

        public ProdutosService() { }

        public IEnumerable<ProdutosModel> GetAll() => _produtos;

        public ProdutosModel? GetById(string id) => _produtos.FirstOrDefault(p => p.Codigo == id);

        public void Add(ProdutosModel produto) => _produtos.Add(produto);
    }
}
