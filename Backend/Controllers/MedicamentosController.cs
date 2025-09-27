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
        public async Task< ActionResult<IEnumerable<MedicamentosModel>>>Get()
        {
            var medicamentos = await _repository.Get();

            return Ok(medicamentos);
        }


        [HttpGet("{id:int}")]

        public async Task< ActionResult<MedicamentosModel>> GetMedicamentoById(int id)
        {

            var medicamentos =await _repository.GetMedicamento(id);

            return Ok(medicamentos);

        }


        [HttpPost]
        public async Task<ActionResult> Post(MedicamentosModel medicamento)
        {

         
            var medicamentoCriado =await _repository.CreateMedicamento(medicamento);

            return CreatedAtAction(
                    nameof(GetMedicamentoById),
                    new {id = medicamentoCriado.CodigoId},
                    medicamentoCriado
                    
                );
        }


        [HttpPut]

        public async Task<ActionResult> Put(MedicamentosModel medicamento)
        {
            var medicamentoCriado = await _repository.UpdateMedicamento(medicamento);

            return Ok(medicamentoCriado);
        }
       

        [HttpDelete("{id:int}")]

        public async Task<ActionResult> Delete(int id)
        {
            
            var medicamentoExcluido = await _repository.DeleteMedicamento(id);
      
            return Ok(medicamentoExcluido);


        }
    }
}
