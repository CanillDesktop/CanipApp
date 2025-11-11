using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Protegendo os endpoints
    public class InsumosController : ControllerBase
    {
        private readonly IInsumosService _insumosService;

        public InsumosController(IInsumosService insumosService)
        {
            _insumosService = insumosService;
        }

        [HttpGet]
        public async Task<IActionResult> BuscarTodos()
        {
            var insumos = await _insumosService.BuscarTodosAsync();
            return Ok(insumos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            var insumo = await _insumosService.BuscarPorIdAsync(id);
            if (insumo == null)
            {
                return NotFound();
            }
            return Ok(insumo);
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] InsumosDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var novoInsumo = await _insumosService.CriarAsync(dto);
            // Retorna o objeto criado e um status 201 (Created)
            return CreatedAtAction(nameof(BuscarPorId), new { id = novoInsumo.CodigoId }, novoInsumo);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] InsumosDTO dto)
        {
            if (id != dto.CodigoId)
            {
                return BadRequest("O ID da rota e o ID do objeto não correspondem.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var insumoAtualizado = await _insumosService.AtualizarAsync(dto);
            if (insumoAtualizado == null)
            {
                return NotFound();
            }

            return Ok(insumoAtualizado); // Pode retornar Ok() ou o objeto (Ok(insumoAtualizado))
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deletar(int id)
        {
            var sucesso = await _insumosService.DeletarAsync(id);
            if (!sucesso)
            {
                return NotFound();
            }

            return NoContent(); // Sucesso, sem conteúdo para retornar
        }
    }
}