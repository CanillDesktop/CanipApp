using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InsumosController : ControllerBase

    {
        private readonly IInsumosService _service;
        public InsumosController(IInsumosService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InsumosDTO>>> Get()
        {
            var insumos = await _service.BuscarTodosAsync();
            return Ok(insumos);

        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<InsumosDTO>> GetById(int id)
        {
            var insumo = await _service.BuscarPorIdAsync(id);
            if (insumo == null)
            {
                return NotFound($"Medicamento com o ID {id} não foi encontrado.");
            }
            return Ok(insumo);
        }


        [HttpPost]
        public async Task<ActionResult<InsumosDTO>> Post(InsumosDTO insumoDto)
        {
            var novoMedicamento = await _service.CriarAsync(insumoDto);
            return CreatedAtAction(nameof(GetById), new { id = novoMedicamento.CodigoId }, novoMedicamento);
        }


        [HttpPut]
        public async Task<ActionResult<InsumosDTO>> Put(InsumosDTO insumoDto)
        {
            var insumosAtualizado = await _service.AtualizarAsync(insumoDto);
            if (insumosAtualizado == null)
            {
                return NotFound($"Medicamento com o ID não foi encontrado.");
            }
            return Ok(insumosAtualizado);
        }


        [HttpDelete("{id:int}")]

        public async Task<ActionResult<MedicamentoDTO>> Delete(int id)
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



