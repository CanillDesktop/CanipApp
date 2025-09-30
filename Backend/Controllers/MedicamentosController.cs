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
        public  ActionResult<IEnumerable<MedicamentosModel>>Get()
        {
            var medicamentos = _repository.Get();

            return Ok(medicamentos);
        }


        [HttpGet("{id:int}")]

        public  ActionResult<MedicamentosModel> Get(int id)
        {

            var medicamentos = _repository.GetMedicamento(id);

            if (medicamentos is null)
            {
                return NotFound(id);
            }

            return Ok();//medicamentos;

        }


        [HttpPost]
        public ActionResult Post(MedicamentosModel medicamento)
        {

            if (medicamento is null)
            {
                return BadRequest("Dados invalidos");

            }
            var medicamentoCriado =_repository.UpdateMedicamento(medicamento);

            return Ok();// new CreatedAtRouteResult("Medicamento Obtido", new { id = medicamentoCriado.CodigoId }, medicamentoCriado);
        }

       

        [HttpDelete("{id:int}")]

        public ActionResult Delete(int id)
        {
            var medicamentoCriado = _repository.GetMedicamento(id);
            if (medicamentoCriado is null)
            {
                return NotFound("Medicamento nao encontrado na base de dados");
            }

            var medicamentoExcluido = _repository.DeleteMedicamento(id);
      
            return Ok(medicamentoExcluido);


        }
    }
}
