using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Backend.Models.Medicamentos;

using Backend.Services;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class MedicamentosController : ControllerBase
    {
        private readonly IMedicamentosService _service;

        public MedicamentosController(IMedicamentosService service)
        {
           _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicamentoDTO>>> Get()
        {
            var medicamentos = await _service.RetornarMedicamentos();
            return Ok(medicamentos);

        }


        [HttpGet("{id:int}")]

        public async Task<ActionResult<MedicamentoDTO>> GetMedicamentoById(int id)
        {
            var medicamento = await _service.RetornarMedicamentoId(id);
            if (medicamento == null)
            {
                return NotFound($"Medicamento com o ID {id} não foi encontrado.");
            }
            return Ok(medicamento);
        }


        [HttpPost]
        public async Task<ActionResult<MedicamentoDTO>> Post(MedicamentoDTO medicamentoDto)
        {
            var novoMedicamento = await _service.CriarMedicamento(medicamentoDto);
            return CreatedAtAction(nameof(GetMedicamentoById), new { id = novoMedicamento.CodigoId }, novoMedicamento);
        }


        [HttpPut]

        public async Task<ActionResult<MedicamentoDTO>> Put(MedicamentoDTO medicamentoDto)
        {
            var medicamentoAtualizado = await _service.AtualizarMedicamento(medicamentoDto);
            if (medicamentoAtualizado == null)
            {
                return NotFound($"Medicamento com o ID não foi encontrado.");
            }
            return Ok(medicamentoAtualizado);
        }
       

        [HttpDelete("{id:int}")]

        public async Task<ActionResult<MedicamentoDTO>> Delete(int id)
        {
            var sucesso = await _service.DeletarMedicamento(id);
            if (!sucesso)
            {
                return NotFound($"Medicamento com o ID {id} não foi encontrado.");
            }
          
            return NoContent();
        }
    }
}
