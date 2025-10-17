using Backend.Models.Medicamentos;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
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

        public async Task<IEnumerable<MedicamentoDTO>> BuscarTodosAsync()
        {
            var medicamentos = await _repository.GetAsync();

            return medicamentos.Select(model => MapModelToDto(model));
        }

        public async Task<MedicamentoDTO?> BuscarPorIdAsync(int id)
        {
            var medicamento = await _repository.GetByIdAsync(id);
            if (medicamento == null)
            {
                return null;
            }

            return MapModelToDto(medicamento);
        }

        public async Task<MedicamentoDTO> CriarAsync(MedicamentoDTO? medicamentoDto)
        {
            var medicamentoModel = MapDtoToModel(medicamentoDto);
            var novoMedicamento = await _repository.CreateAsync(medicamentoModel);

            return MapModelToDto(novoMedicamento);
        }

        public async Task<MedicamentoDTO?> AtualizarAsync(MedicamentoDTO? medicamentoDto)
        {
            var medicamentoExistente = await _repository.GetByIdAsync(medicamentoDto.CodigoId);
            if (medicamentoExistente == null)
            {
                return null;
            }

            PersistirModel(medicamentoExistente, medicamentoDto);


            var medicamentoAtualizado = await _repository.UpdateAsync(medicamentoExistente);
            return medicamentoAtualizado != null ? MapModelToDto(medicamentoAtualizado) : null;
        }

        public async Task<bool> DeletarAsync(int id)
        {
            var medicamento = await _repository.GetByIdAsync(id);
            if (medicamento == null)
            {
                return false;
            }
            await _repository.DeleteAsync(id);
            return true;
        }

        private static MedicamentoDTO MapModelToDto(MedicamentosModel model)
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

        private static MedicamentosModel MapDtoToModel(MedicamentoDTO dto)
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

        private void PersistirModel(MedicamentosModel model, MedicamentoDTO modelDto)
        {
            model.CodigoId = modelDto.CodigoId;
            model.Prioridade = modelDto.Prioridade;
            model.DescricaoMedicamentos = modelDto.DescricaoMedicamentos;
            model.NotaFiscal = modelDto.NotaFiscal;
            model.NomeComercial = modelDto.NomeComercial;
            model.PublicoAlvo = modelDto.PublicoAlvo;
            model.DataDeEntradaDoMedicamento = modelDto.DataDeEntradaDoMedicamento;
            model.ConsumoMensal = modelDto.ConsumoMensal;
            model.ConsumoAnual = modelDto.ConsumoAnual;
            model.ValidadeMedicamento = modelDto.ValidadeMedicamento;
            model.EstoqueDisponivel = modelDto.EstoqueDisponivel;
            model.EntradaEstoque = modelDto.EntradaEstoque;
            model.EntradaEstoque = modelDto.EntradaEstoque;
        }


    }
}