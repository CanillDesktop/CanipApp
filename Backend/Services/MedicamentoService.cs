using Backend.Models.Medicamentos;
using Backend.Repositories.Interfaces; // (Namespace do seu IMedicamentosRepository)
using Backend.Services.Interfaces;
using Shared.DTOs;
using System.Globalization;

namespace Backend.Services
{
    public class MedicamentosService : IMedicamentosService
    {
        // Agora injeta o repositório que fala com o SQLite
        private readonly IMedicamentosRepository _medicamentosRepository;

        public MedicamentosService(IMedicamentosRepository medicamentosRepository)
        {
            _medicamentosRepository = medicamentosRepository;
        }

        public async Task<IEnumerable<MedicamentoDTO>> BuscarTodosAsync()
        {
            // 1. Busca MODELOS do repositório local
            // (Adicionando filtro de soft delete)
            var models = await _medicamentosRepository.BuscarTodosAsync(m => m.IsDeleted == false);

            // 2. Converte Modelos para DTOs para o Controller
            return models.Select(model => ToDTO(model));
        }

        public async Task<MedicamentoDTO?> BuscarPorIdAsync(int id)
        {
            var model = await _medicamentosRepository.BuscarPorIdAsync(id);

            // Ignora se estiver deletado
            if (model == null || model.IsDeleted)
            {
                return null;
            }

            return ToDTO(model);
        }

        public async Task<MedicamentoDTO> CriarAsync(MedicamentoDTO medicamentoDto)
        {
            // 1. Converte DTO para Modelo
            var model = ToModel(medicamentoDto);

            // 2. Define o timestamp para sincronização
            model.DataAtualizacao = DateTime.UtcNow;
            model.IsDeleted = false; // Garante que não está deletado

            // 3. Salva Modelo no repositório local
            var novoModel = await _medicamentosRepository.CriarAsync(model);

            // 4. Converte Modelo de volta para DTO
            return ToDTO(novoModel);
        }

        public async Task<MedicamentoDTO?> AtualizarAsync(MedicamentoDTO medicamentoDto)
        {
            var modelExistente = await _medicamentosRepository.BuscarPorIdAsync(medicamentoDto.CodigoId);
            if (modelExistente == null || modelExistente.IsDeleted)
            {
                return null;
            }

            // 1. Converte DTO para Modelo
            var modelParaAtualizar = ToModel(medicamentoDto);

            // 2. Define o timestamp para sincronização
            modelParaAtualizar.DataAtualizacao = DateTime.UtcNow;
            modelParaAtualizar.IsDeleted = modelExistente.IsDeleted; // Mantém o status de delete

            // 3. Atualiza Modelo no repositório local
            await _medicamentosRepository.AtualizarAsync(modelParaAtualizar);

            return ToDTO(modelParaAtualizar);
        }

        public async Task<bool> DeletarAsync(int id)
        {
            // Implementando SOFT DELETE
            var model = await _medicamentosRepository.BuscarPorIdAsync(id);
            if (model == null || model.IsDeleted) // Se já foi deletado, retorna falso
            {
                return false;
            }

            // 1. Marca como deletado
            model.IsDeleted = true;
            model.DataAtualizacao = DateTime.UtcNow; // ESSENCIAL para sincronizar

            // 2. Chama ATUALIZAR (não deletar)
            await _medicamentosRepository.AtualizarAsync(model);
            return true;
        }


        // --- MÉTODOS PRIVADOS DE MAPEAMENTO (Permanecem os mesmos) ---

        private MedicamentosModel ToModel(MedicamentoDTO dto)
        {
            return new MedicamentosModel
            {
                CodigoId = dto.CodigoId,
                Prioridade = (Shared.Enums.PrioridadeEnum)dto.Prioridade,
                DescricaoMedicamentos = dto.DescricaoMedicamentos,
                DataDeEntradaDoMedicamento = (DateTime)dto.DataDeEntradaDoMedicamento,
                NotaFiscal = dto.NotaFiscal,
                NomeComercial = dto.NomeComercial,
                PublicoAlvo = (Shared.Enums.PublicoAlvoMedicamentoEnum)dto.PublicoAlvo,
                ConsumoMensal = (int)dto.ConsumoMensal,
                ConsumoAnual = (int)dto.ConsumoAnual,
                ValidadeMedicamento = dto.ValidadeMedicamento,
                EstoqueDisponivel = dto.EstoqueDisponivel,
                EntradaEstoque = dto.EntradaEstoque,
                SaidaTotalEstoque = dto.SaidaTotalEstoque
                // DataAtualizacao e IsDeleted são definidos nos métodos de CRUD
            };
        }

        private MedicamentoDTO ToDTO(MedicamentosModel model)
        {
            DateOnly? validade = null;
            if (!string.IsNullOrEmpty(model.ValidadeMedicamentoString) &&
                DateOnly.TryParseExact(model.ValidadeMedicamentoString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                validade = parsedDate;
            }

            return new MedicamentoDTO
            {
                CodigoId = model.CodigoId,
                Prioridade = model.Prioridade,
                DescricaoMedicamentos = model.DescricaoMedicamentos,
                DataDeEntradaDoMedicamento = model.DataDeEntradaDoMedicamento,
                NotaFiscal = model.NotaFiscal,
                NomeComercial = model.NomeComercial,
                PublicoAlvo = model.PublicoAlvo,
                ConsumoMensal = model.ConsumoMensal,
                ConsumoAnual = model.ConsumoAnual,
                ValidadeMedicamento = validade,
                EstoqueDisponivel = model.EstoqueDisponivel,
                EntradaEstoque = model.EntradaEstoque,
                SaidaTotalEstoque = model.SaidaTotalEstoque
            };
        }
    }
}