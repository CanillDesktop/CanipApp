using Backend.Models.Medicamentos;
using Backend.Repositories;
using NuGet.Protocol.Core.Types;
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

        public async Task<IEnumerable<InsumosDTO>> RetornarInsumos()
        {
            var insumos = await _repository.Get();

            return insumos.Select(model => MapModelToDto(model));
        }

        public async Task<InsumosDTO?> RetornarInsumoId(int id)
        {
            var insumo= await _repository.GetInsumo(id);
            if (insumo == null)
            {
                return null;
            }

            return MapModelToDto(insumo);
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
                Unidade=model.Unidade,
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



    }
}
