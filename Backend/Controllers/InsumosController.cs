using Backend.Models.Insumos;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Insumos;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InsumosController : ControllerBase
    {
        private readonly IInsumosService _insumosService;

        public InsumosController(IInsumosService insumosService)
        {
        private readonly IInsumosService _service;
        public InsumosController(IInsumosService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InsumosCadastroDTO>>> Get([FromQuery] InsumosFiltroDTO filtro)
        {
            var filteredRequest = HttpContext.Request.GetDisplayUrl().Contains('?');

            if (filteredRequest)
                return Ok(await _service.BuscarTodosAsync(filtro));
            else
                return Ok(await _service.BuscarTodosAsync());
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<InsumosCadastroDTO>> GetById(int id)
        {
            var insumo = await _service.BuscarPorIdAsync(id);
            if (insumo == null)
            {
                return NotFound($"Insumo com o ID {id} não foi encontrado.");
            }
            return Ok(insumo);
        }


        [HttpPost]
        public async Task<ActionResult<InsumosCadastroDTO>> Post(InsumosCadastroDTO insumoDto)
        {
            InsumosModel model = insumoDto;
            var novoInsumo = await _service.CriarAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = novoInsumo.IdItem }, novoInsumo);
            }

            var novoInsumo = await _insumosService.CriarAsync(dto);
            // Retorna o objeto criado e um status 201 (Created)
            return CreatedAtAction(nameof(BuscarPorId), new { id = novoInsumo.CodigoId }, novoInsumo);
        }

        [HttpPut]
        public async Task<ActionResult<InsumosCadastroDTO>> Put(InsumosCadastroDTO insumoDto)
            {
            var insumosAtualizado = await _service.AtualizarAsync(insumoDto);
            if (insumosAtualizado == null)
            {
                return NotFound($"Insumo com o ID não foi encontrado.");
            }
            return Ok(insumosAtualizado);
            }

            return Ok(insumoAtualizado); // Pode retornar Ok() ou o objeto (Ok(insumoAtualizado))
        }

        [HttpDelete("{id:int}")]

        public async Task<ActionResult<bool>> Delete(int id)
        {
            var sucesso = await _service.DeletarAsync(id);
            if (!sucesso)
            {
                return NotFound($"Insumo com o ID {id} não foi encontrado.");
            }

            return NoContent(); 
        }
    }
}



