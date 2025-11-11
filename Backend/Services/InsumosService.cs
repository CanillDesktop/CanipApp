using Backend.Models.Insumos;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Humanizer;
using Shared.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class InsumosService : IInsumosService
    {
        private readonly IInsumosRepository _insumosRepository;

        public InsumosService(IInsumosRepository insumosRepository)
        {
            _insumosRepository = insumosRepository;
        }

        public async Task<IEnumerable<InsumosDTO>> BuscarTodosAsync()
        {
            // 1. Busca MODELOS (filtrando soft delete)
            var models = await _insumosRepository.BuscarTodosAsync(m => m.IsDeleted == false);

            // 2. Converte Modelos para DTOs
            return models.Select(model => ToDTO(model));
        }

        public async Task<InsumosDTO?> BuscarPorIdAsync(int id)
        {
            var model = await _insumosRepository.BuscarPorIdAsync(id);

            // Ignora se estiver deletado
            if (model == null || model.IsDeleted)
            {
                return null;
            }

            return ToDTO(model);
        }

        public async Task<InsumosDTO> CriarAsync(InsumosDTO dto)
        {
            // 1. Converte DTO para Modelo
            var model = ToModel(dto);

            // 2. Define o timestamp e garante IsDeleted = false
            model.DataAtualizacao = DateTime.UtcNow;
            model.IsDeleted = false;

            // 3. Salva Modelo no repositório local
            var novoModel = await _insumosRepository.CriarAsync(model);

            // 4. Converte Modelo de volta para DTO
            return ToDTO(novoModel);
        }

        public async Task<InsumosDTO?> AtualizarAsync(InsumosDTO dto)
        {
            var modelExistente = await _insumosRepository.BuscarPorIdAsync(dto.CodigoId);
            if (modelExistente == null || modelExistente.IsDeleted)
            {
                return null; // Não pode atualizar item inexistente ou deletado
            }

            // 1. Converte DTO para Modelo
            var modelParaAtualizar = ToModel(dto);

            // 2. Define o timestamp e mantém status de delete
            modelParaAtualizar.DataAtualizacao = DateTime.UtcNow;
            modelParaAtualizar.IsDeleted = modelExistente.IsDeleted; // Mantém o status

            // Garante que a Chave Primária não seja modificada (o EF Core não gosta disso)
            // O ToModel já deve ter feito isso, mas é uma garantia.
            modelParaAtualizar.CodigoId = modelExistente.CodigoId;


            // 3. Atualiza Modelo no repositório local
            await _insumosRepository.AtualizarAsync(modelParaAtualizar);

            return ToDTO(modelParaAtualizar);
        }

        public async Task<bool> DeletarAsync(int id)
        {
            // Implementando SOFT DELETE (igual ProdutosService)
            var model = await _insumosRepository.BuscarPorIdAsync(id);
            if (model == null || model.IsDeleted) // Se não existe ou já foi deletado
            {
                return false;
            }

            // 1. Marca como deletado
            model.IsDeleted = true;
            model.DataAtualizacao = DateTime.UtcNow; // ESSENCIAL para sincronizar

            // 2. Chama ATUALIZAR (não deletar)
            await _insumosRepository.AtualizarAsync(model);
            return true;
        }

        // --- MÉTODOS PRIVADOS DE MAPEAMENTO ---

        private InsumosModel ToModel(InsumosDTO dto)
        {
            return new InsumosModel
            {
                CodigoId = dto.CodigoId, // Inclui o ID para atualizações
                DescricaoSimplificada = dto.DescricaoSimplificada,
                DescricaoDetalhada = dto.DescricaoDetalhada,
                DataDeEntradaDoInsumo = dto.DataDeEntradaDoInsumo,
                NotaFiscal = dto.NotaFiscal,
                Unidade = dto.Unidade,
                ConsumoMensal = dto.ConsumoMensal,
                ConsumoAnual = dto.ConsumoAnual,
                ValidadeInsumo = dto.ValidadeInsumo,
                EstoqueDisponivel = dto.EstoqueDisponivel,
                EntradaEstoque = dto.EntradaEstoque,
                SaidaTotalEstoque = dto.SaidaTotalEstoque
                // DataAtualizacao e IsDeleted são definidos nos métodos de CRUD
            };
        }

        private InsumosDTO ToDTO(InsumosModel model)
        {
            return new InsumosDTO
            {
                CodigoId = model.CodigoId,
                DescricaoSimplificada = model.DescricaoSimplificada,
                DescricaoDetalhada = model.DescricaoDetalhada,
                DataDeEntradaDoInsumo = model.DataDeEntradaDoInsumo,
                NotaFiscal = model.NotaFiscal,
                Unidade = model.Unidade,
                ConsumoMensal = model.ConsumoMensal,
                ConsumoAnual = model.ConsumoAnual,
                ValidadeInsumo = model.ValidadeInsumo,
                EstoqueDisponivel = model.EstoqueDisponivel,
                EntradaEstoque = model.EntradaEstoque,
                SaidaTotalEstoque = model.SaidaTotalEstoque
            };
        }
    }
}