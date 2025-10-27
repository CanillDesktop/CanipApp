using Backend.Models.Medicamentos;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Shared.DTOs;

namespace Backend.Services
{
    public class InsumosService : IInsumosService
    {
        public readonly IInsumosRepository _repository;


        public InsumosService(IInsumosRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<InsumosDTO>> BuscarTodosAsync()
        {
            var insumos = await _repository.GetAsync();

            return insumos.Select(model => MapModelToDto(model));
        }

        public async Task<InsumosDTO?> BuscarPorIdAsync(int id)
        {
            var insumo = await _repository.GetByIdAsync(id);
            if (insumo == null)
            {
                return null;
            }

            return MapModelToDto(insumo);
        }


        public async Task<InsumosDTO?> CriarAsync(InsumosDTO insumosDto)
        {

            var insumoModel = MapDtoToModel(insumosDto);
            var novoInsumo = await _repository.CreateAsync(insumoModel);

            return MapModelToDto(novoInsumo);
        }
        public async Task<InsumosDTO?> AtualizarAsync(InsumosDTO insumosDto)
        {
            var insumoExistente = await _repository.GetByIdAsync(insumosDto.CodigoId);
            if (insumoExistente == null)
            {
                return null;
            }

            PersistirModel(insumoExistente, insumosDto);


            var insumoAtualizado = await _repository.UpdateAsync(insumoExistente);
            return MapModelToDto(insumoAtualizado!);
        }

        public async Task<bool> DeletarAsync(int id)
        {
            var insumo = await _repository.GetByIdAsync(id);
            if (insumo == null)
            {
                return false;
            }
            return await _repository.DeleteAsync(id);
        }

        private static InsumosDTO MapModelToDto(InsumosModel model)
        {
            return new InsumosDTO
            {
                CodigoId = model.CodigoId,
                DescricaoSimplificada = model.DescricaoSimplificada,
                DescricaoDetalhada = model.DescricaoDetalhada,
                DataDeEntradaDoMedicamento = model.DataDeEntradaDoMedicamento,
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


        private static InsumosModel MapDtoToModel(InsumosDTO dto)
        {
            return new InsumosModel
            {
                CodigoId = dto.CodigoId,
                DescricaoSimplificada = dto.DescricaoSimplificada,
                DataDeEntradaDoMedicamento = dto.DataDeEntradaDoMedicamento,
                DescricaoDetalhada = dto.DescricaoDetalhada,
                NotaFiscal = dto.NotaFiscal,
                Unidade = dto.Unidade,
                ConsumoMensal = dto.ConsumoMensal,
                ConsumoAnual = dto.ConsumoAnual,
                ValidadeInsumo = dto.ValidadeInsumo,
                EstoqueDisponivel = dto.EstoqueDisponivel,
                EntradaEstoque = dto.EntradaEstoque,
                SaidaTotalEstoque = dto.SaidaTotalEstoque
            };
        }

        private void PersistirModel(InsumosModel model, InsumosDTO modelDto)
        {
            model.CodigoId = modelDto.CodigoId;

            model.DescricaoDetalhada = modelDto.DescricaoDetalhada;
            model.DescricaoSimplificada = modelDto.DescricaoSimplificada;
            model.NotaFiscal = modelDto.NotaFiscal;
            model.Unidade = modelDto.Unidade;
            model.DataDeEntradaDoMedicamento = modelDto.DataDeEntradaDoMedicamento;
            model.ConsumoMensal = modelDto.ConsumoMensal;
            model.ConsumoAnual = modelDto.ConsumoAnual;
            model.ValidadeInsumo = modelDto.ValidadeInsumo;
            model.EstoqueDisponivel = modelDto.EstoqueDisponivel;
            model.EntradaEstoque = modelDto.EntradaEstoque;
            model.EntradaEstoque = modelDto.EntradaEstoque;
        }
    }
}