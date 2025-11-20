using Backend.Models.Medicamentos;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Medicamentos;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicamentosController : ControllerBase
    {
        private readonly IMedicamentosService _service;

        public MedicamentosController(IMedicamentosService service)
        {
           _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicamentoCadastroDTO>>> Get([FromQuery] MedicamentosFiltroDTO filtro)
        {
            var filteredRequest = HttpContext.Request.GetDisplayUrl().Contains('?');

            if (filteredRequest)
                return Ok(await _service.BuscarTodosAsync(filtro));
            else
                return Ok(await _service.BuscarTodosAsync());
        }


        [HttpGet("{id:int}")]

        public async Task<ActionResult<MedicamentoCadastroDTO>> GetById(int id)
        {
            var medicamento = await _service.BuscarPorIdAsync(id);
            if (medicamento == null)
            {
                return NotFound($"Medicamento com o ID {id} não foi encontrado.");
            }
            return Ok(medicamento);
        }


        [HttpPost]
        public async Task<ActionResult<MedicamentoCadastroDTO>> Post(MedicamentoCadastroDTO medicamentoDto)
        {
            MedicamentosModel model = medicamentoDto;
            var novoMedicamento = await _service.CriarAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = novoMedicamento.IdItem }, novoMedicamento);
        }


        [HttpPut]

        public async Task<ActionResult<MedicamentoCadastroDTO>> Put(MedicamentoCadastroDTO medicamentoDto)
        {
            var medicamentoAtualizado = await _service.AtualizarAsync(medicamentoDto);
            if (medicamentoAtualizado == null)
            {
                return NotFound($"Medicamento com o ID não foi encontrado.");
            }
            return Ok(medicamentoAtualizado);
        }
       

        [HttpDelete("{id:int}")]

        public async Task<ActionResult<MedicamentoCadastroDTO>> Delete(int id)
        {
            var sucesso = await _service.DeletarAsync(id);
            if (!sucesso)
            {
                return NotFound($"Medicamento com o ID {id} não foi encontrado.");
            }
          
            return NoContent();
        }
    }
}
