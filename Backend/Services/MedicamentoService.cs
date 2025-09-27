using Backend.Models.Medicamentos;
using Backend.Repositories;
using Shared.DTOs;

namespace Backend.Services
{
    public class MedicamentosService : IMedicamentosService
    {
        private readonly IMedicamentosRepository _repository;

        public MedicamentosService(IMedicamentosRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MedicamentoDTO>> GetAllMedicamentos()
        {
            var medicamentos = await _repository.Get();
 
            return medicamentos.Select(model => MapModelToDto(model));
        }

        public async Task<MedicamentoDTO?> GetMedById(int id)
        {
            var medicamento = await _repository.GetMedicamento(id);
            if (medicamento == null)
            {
                return null;
            }
           
            return MapModelToDto(medicamento);
        }

        public async Task<MedicamentoDTO> CreateMedicamento(MedicamentoDTO medicamentoDto)
        {
            // Mapeia o DTO para Model antes de enviar para o repositório
            var medicamentoModel = MapDtoToModel(medicamentoDto);
            var novoMedicamento = await _repository.CreateMedicamento(medicamentoModel);
            // Retorna o DTO do objeto criado
            return MapModelToDto(novoMedicamento);
        }

        public async Task<MedicamentoDTO?> UpdateMedicamento( MedicamentoDTO medicamentoDto)
        {
            var medicamentoExistente = await _repository.GetMedicamento(medicamentoDto.CodigoId);
            if (medicamentoExistente == null)
            {
                return null;
            }

            // Atualiza o modelo existente com os dados do DTO
            medicamentoExistente.Prioridade = medicamentoDto.Prioridade;
            medicamentoExistente.DescricaoMedicamentos = medicamentoDto.DescricaoMedicamentos;
            medicamentoExistente.DataDeEntradaDoMedicamento = medicamentoDto.DataDeEntradaDoMedicamento;
            medicamentoExistente.NotaFiscal = medicamentoDto.NotaFiscal;
            medicamentoExistente.NomeComercial = medicamentoDto.NomeComercial;
            medicamentoExistente.PublicoAlvo = medicamentoDto.PublicoAlvo;
            medicamentoExistente.ConsumoMensal = medicamentoDto.ConsumoMensal;
            medicamentoExistente.ConsumoAnual = medicamentoDto.ConsumoAnual;
            medicamentoExistente.ValidadeMedicamento = medicamentoDto.ValidadeMedicamento;
            medicamentoExistente.EstoqueDisponivel = medicamentoDto.EstoqueDisponivel;
            medicamentoExistente.EntradaEstoque = medicamentoDto.EntradaEstoque;
            medicamentoExistente.SaidaTotalEstoque = medicamentoDto.SaidaTotalEstoque;

            var medicamentoAtualizado = await _repository.UpdateMedicamento(medicamentoExistente);
            return MapModelToDto(medicamentoAtualizado);
        }

        public async Task<bool> DeleteMedicamento(int id)
        {
            var medicamento = await _repository.GetMedicamento(id);
            if (medicamento == null)
            {
                return false;
            }
            await _repository.DeleteMedicamento(id);
            return true;
        }

        // --- Métodos de Mapeamento Privados ---

        private MedicamentoDTO MapModelToDto(MedicamentosModel model)
        {
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
                ValidadeMedicamento = model.ValidadeMedicamento,
                EstoqueDisponivel = model.EstoqueDisponivel,
                EntradaEstoque = model.EntradaEstoque,
                SaidaTotalEstoque = model.SaidaTotalEstoque
            };
        }

        private MedicamentosModel MapDtoToModel(MedicamentoDTO dto)
        {
            return new MedicamentosModel
            {
                CodigoId = dto.CodigoId,
                Prioridade = dto.Prioridade,
                DescricaoMedicamentos = dto.DescricaoMedicamentos,
                DataDeEntradaDoMedicamento = dto.DataDeEntradaDoMedicamento,
                NotaFiscal = dto.NotaFiscal,
                NomeComercial = dto.NomeComercial,
                PublicoAlvo = dto.PublicoAlvo,
                ConsumoMensal = dto.ConsumoMensal,
                ConsumoAnual = dto.ConsumoAnual,
                ValidadeMedicamento = dto.ValidadeMedicamento,
                EstoqueDisponivel = dto.EstoqueDisponivel,
                EntradaEstoque = dto.EntradaEstoque,
                SaidaTotalEstoque = dto.SaidaTotalEstoque
            };
        }
    }
}