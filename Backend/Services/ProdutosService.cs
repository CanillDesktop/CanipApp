using Backend.Models.Produtos;

namespace Backend.Services
{
    public class ProdutosService
    {
        private static readonly List<ProdutosModel> _produtos = [];

        public ProdutosService() { }

        public IEnumerable<ProdutosModel> BuscarTodos() => _produtos;

        public ProdutosModel? BuscaPorId(string id) => _produtos.FirstOrDefault(p => p.IdProduto == id);

        public void CriaProduto(ProdutosModel? model)
        {
            ArgumentNullException.ThrowIfNull(model);

            _produtos.Add(model);
        }

        public void Atualizar(string id, ProdutosModel model)
        {
            var produto = _produtos.Find(p => p.IdProduto == id) ?? throw new ArgumentNullException(nameof(model));

            produto = model;
        }

        public void Deletar(string id)
        {
            var produto = _produtos.Find(p => p.IdProduto == id) ?? throw new ArgumentNullException();

            _produtos.Remove(produto);
        }
    }
}
