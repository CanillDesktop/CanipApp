using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuariosService _service;

        public UsuariosController(IUsuariosService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<UsuarioRequestDTO>> Create([FromBody] UsuarioRequestDTO dto)
        {
            try
            {
                var usuarioCriado = await _service.CriarAsync(dto);

                if (usuarioCriado == null)
                    return BadRequest(new { error = "Não foi possível criar o usuário." });

                // 🔥 RETORNA DIRETAMENTE O USUÁRIO CRIADO, SEM CreatedAtAction
                return Ok(usuarioCriado);
            }
            catch (ArgumentNullException)
            {
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDTO>>>? Get()
        {
            return Ok(await _service.BuscarTodosAsync());
        }

        [HttpGet("{id:int}")] // 🔥 REMOVIDO [Authorize] TEMPORARIAMENTE
        public async Task<ActionResult<UsuarioResponseDTO>> GetById(int id)
        {
            var usuario = await _service.BuscarPorIdAsync(id);

            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] UsuarioRequestDTO dto)
        {
            try
            {
                dto.Id = id;
                await _service.AtualizarAsync(dto);

                return NoContent();
            }
            catch (ArgumentNullException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeletarAsync(id);

                return NoContent();
            }
            catch (ArgumentNullException)
            {
                return NotFound();
            }
        }
    }
}