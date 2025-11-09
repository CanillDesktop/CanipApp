using Backend.Models.Medicamentos;
using Backend.Models.Produtos;
using Backend.Repositories.Interfaces; // (Namespace do seu IMedicamentosRepository)
using Backend.Services.Interfaces;
using Shared.DTOs;
using System.Globalization;

namespace Backend.Services
{
    public class ProdutosService : IProdutosService
    {
        // Agora injeta o repositório que fala com o SQLite
        private readonly IProdutosRepository _produtosRepository;

        public ProdutosService(IProdutosRepository produtosRepository)
        {
            _produtosRepository = produtosRepository;
        }

        public async Task<IEnumerable<ProdutosDTO>> BuscarTodosAsync()
        {
            // 1. Busca MODELOS do repositório local
            // (Adicionando filtro de soft delete)
            var models = await _produtosRepository.BuscarTodosAsync(m => m.IsDeleted == false);

            // 2. Converte Modelos para DTOs para o Controller
            return models.Select(model => ToDTO(model));
        }

        public async Task<ProdutosDTO?> BuscarPorIdAsync(string id)
        {
         

            var model = await _produtosRepository.BuscarPorIdAsync(id);

            // Ignora se estiver deletado
            if (model == null || model.IsDeleted)
            {
                return null;
            }

            return ToDTO(model);
        }

        public async Task<ProdutosDTO> CriarAsync(ProdutosDTO medicamentoDto)
        {
            // 1. Converte DTO para Modelo
            var model = ToModel(medicamentoDto);

            // 2. Define o timestamp para sincronização
            model.DataAtualizacao = DateTime.UtcNow;
            model.IsDeleted = false; // Garante que não está deletado

            // 3. Salva Modelo no repositório local
            var novoModel = await _produtosRepository.CriarAsync(model);

            // 4. Converte Modelo de volta para DTO
            return ToDTO(novoModel);
        }

        public async Task<ProdutosDTO?> AtualizarAsync(ProdutosDTO produtoDto)
        {
            var IdProdutoaux = produtoDto.IdProduto;
            var modelExistente = await _produtosRepository.BuscarPorIdAsync(IdProdutoaux);
            if (modelExistente == null || modelExistente.IsDeleted)
            {
                return null;
            }

            // 1. Converte DTO para Modelo
            var modelParaAtualizar = ToModel(produtoDto);

            // 2. Define o timestamp para sincronização
            modelParaAtualizar.DataAtualizacao = DateTime.UtcNow;
            modelParaAtualizar.IsDeleted = modelExistente.IsDeleted; // Mantém o status de delete

            // 3. Atualiza Modelo no repositório local
            await _produtosRepository.AtualizarAsync(modelParaAtualizar);

            return ToDTO(modelParaAtualizar);
        }

        public async Task<bool> DeletarAsync(string id)
        {
         
            // Implementando SOFT DELETE
            var model = await _produtosRepository.BuscarPorIdAsync(id);
            if (model == null || model.IsDeleted) // Se já foi deletado, retorna falso
            {
                return false;
            }

            // 1. Marca como deletado
            model.IsDeleted = true;
            model.DataAtualizacao = DateTime.UtcNow; // ESSENCIAL para sincronizar

            // 2. Chama ATUALIZAR (não deletar)
            await _produtosRepository.AtualizarAsync(model);
            return true;
        }


        // --- MÉTODOS PRIVADOS DE MAPEAMENTO (Permanecem os mesmos) ---

        private Produtos ToModel(ProdutosDTO dto)
        {
            return new Produtos
            {
                IdProduto = dto.IdProduto,
                DescricaoSimples = dto.DescricaoSimples,
                DescricaoDetalhada = dto.DescricaoDetalhada,
                NFe = dto.NFe,
                Unidade = dto.Unidade,
                Categoria = dto.Categoria,
                Quantidade = dto.Quantidade,
                Validade = dto.Validade,
                EstoqueDisponivel = dto.EstoqueDisponivel
                // DataAtualizacao e IsDeleted são definidos nos métodos de CRUD
            };
        }

        private ProdutosDTO ToDTO(Produtos model)
        {
          
            return new ProdutosDTO
            {
                IdProduto = model.IdProduto,
                DescricaoSimples=model.DescricaoSimples,
                DescricaoDetalhada=model.DescricaoDetalhada,
                NFe = model.NFe,
                Unidade=model.Unidade,
                Categoria = model.Categoria,
                Quantidade = model.Quantidade,
                Validade = model.Validade,
                EstoqueDisponivel = model.EstoqueDisponivel
            };
        }
    }
}