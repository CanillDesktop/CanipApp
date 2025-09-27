using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Backend.Context;
using Backend.Models.Medicamentos;
using Backend.Repositories;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class MedicamentosController : ControllerBase
    {
        private readonly IMedicamentosRepository _repository;

        public MedicamentosController(IMedicamentosRepository repository)
        {
           _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicamentoDTO>>> Get()
        {
            var medicamentos = await _repository.Get();

            var medicamentosDto = medicamentos.Select(i => new MedicamentoDTO
            {
                CodigoId = i.CodigoId,
                Prioridade = i.Prioridade,
                DescricaoMedicamentos = i.DescricaoMedicamentos,
                DataDeEntradaDoMedicamento = i.DataDeEntradaDoMedicamento,
                NotaFiscal = i.NotaFiscal,
                NomeComercial = i.NomeComercial,
                PublicoAlvo = i.PublicoAlvo,
                ConsumoMensal = i.ConsumoMensal,
                ConsumoAnual = i.ConsumoAnual,
                ValidadeMedicamento = i.ValidadeMedicamento,
                EstoqueDisponivel = i.EstoqueDisponivel,
                EntradaEstoque = i.EntradaEstoque,
                SaidaTotalEstoque = i.SaidaTotalEstoque
             }).ToList();
                
                return Ok(medicamentosDto);
            }
        


        [HttpGet("{id:int}")]

        public async Task<ActionResult<MedicamentoDTO>> GetMedicamentoById(int id)
        {

            var medicamentos =await _repository.GetMedicamento(id);

            var medicamentoauxDto = new MedicamentoDTO()
            { 
                CodigoId = medicamentos.CodigoId,
                Prioridade = medicamentos.Prioridade,
                DescricaoMedicamentos = medicamentos.DescricaoMedicamentos,
                DataDeEntradaDoMedicamento = medicamentos.DataDeEntradaDoMedicamento,
                NotaFiscal = medicamentos.NotaFiscal,
                NomeComercial = medicamentos.NomeComercial,
                PublicoAlvo = medicamentos.PublicoAlvo,
                ConsumoMensal = medicamentos.ConsumoMensal,
                ConsumoAnual = medicamentos.ConsumoAnual,
                ValidadeMedicamento = medicamentos.ValidadeMedicamento,
                EstoqueDisponivel = medicamentos.EstoqueDisponivel,
                EntradaEstoque = medicamentos.EntradaEstoque,
                SaidaTotalEstoque = medicamentos.SaidaTotalEstoque
            
            };

            return Ok(medicamentoauxDto);

        }


        [HttpPost]
        public async Task<ActionResult<MedicamentoDTO>> Post(MedicamentoDTO medicamento)
        {
            var medicamentoauxmodel = new MedicamentosModel()
            {
                CodigoId = medicamento.CodigoId,
                Prioridade = medicamento.Prioridade,
                DescricaoMedicamentos = medicamento.DescricaoMedicamentos,
                DataDeEntradaDoMedicamento = medicamento.DataDeEntradaDoMedicamento,
                NotaFiscal = medicamento.NotaFiscal,
                NomeComercial = medicamento.NomeComercial,
                PublicoAlvo = medicamento.PublicoAlvo,
                ConsumoMensal = medicamento.ConsumoMensal,
                ConsumoAnual = medicamento.ConsumoAnual,
                ValidadeMedicamento = medicamento.ValidadeMedicamento,
                EstoqueDisponivel = medicamento.EstoqueDisponivel,
                EntradaEstoque = medicamento.EntradaEstoque,
                SaidaTotalEstoque = medicamento.SaidaTotalEstoque

            };

            var medicamentocriado = await _repository.CreateMedicamento(medicamentoauxmodel);

            return Ok(medicamentocriado);

        }


        [HttpPut]

        public async Task<ActionResult<MedicamentoDTO>> Put(MedicamentoDTO medicamento)
        {
            var medicamentoauxmodel = new MedicamentosModel()
            {
                CodigoId = medicamento.CodigoId,
                Prioridade = medicamento.Prioridade,
                DescricaoMedicamentos = medicamento.DescricaoMedicamentos,
                DataDeEntradaDoMedicamento = medicamento.DataDeEntradaDoMedicamento,
                NotaFiscal = medicamento.NotaFiscal,
                NomeComercial = medicamento.NomeComercial,
                PublicoAlvo = medicamento.PublicoAlvo,
                ConsumoMensal = medicamento.ConsumoMensal,
                ConsumoAnual = medicamento.ConsumoAnual,
                ValidadeMedicamento = medicamento.ValidadeMedicamento,
                EstoqueDisponivel = medicamento.EstoqueDisponivel,
                EntradaEstoque = medicamento.EntradaEstoque,
                SaidaTotalEstoque = medicamento.SaidaTotalEstoque

            };

            var medicamentoCriado = await _repository.UpdateMedicamento(medicamentoauxmodel);

            return Ok(medicamentoCriado);
        }
       

        [HttpDelete("{id:int}")]

        public async Task<ActionResult<MedicamentoDTO>> Delete(int id)
        {
            var medicamentoExcluido = await _repository.DeleteMedicamento(id);

            var medicamentoDtoExcluido = new MedicamentoDTO()
            {
                CodigoId = medicamentoExcluido.CodigoId,
                Prioridade = medicamentoExcluido.Prioridade,
                DescricaoMedicamentos = medicamentoExcluido.DescricaoMedicamentos,
                DataDeEntradaDoMedicamento = medicamentoExcluido.DataDeEntradaDoMedicamento,
                NotaFiscal = medicamentoExcluido.NotaFiscal,
                NomeComercial = medicamentoExcluido.NomeComercial,
                PublicoAlvo = medicamentoExcluido.PublicoAlvo,
                ConsumoMensal = medicamentoExcluido.ConsumoMensal,
                ConsumoAnual = medicamentoExcluido.ConsumoAnual,
                ValidadeMedicamento = medicamentoExcluido.ValidadeMedicamento,
                EstoqueDisponivel = medicamentoExcluido.EstoqueDisponivel,
                EntradaEstoque = medicamentoExcluido.EntradaEstoque,
                SaidaTotalEstoque = medicamentoExcluido.SaidaTotalEstoque

            };

            return Ok(medicamentoExcluido);


        }
    }
}
