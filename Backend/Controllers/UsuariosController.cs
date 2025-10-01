using Backend.Models.Usuarios;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuariosService _service;

        public UsuariosController(UsuariosService service)
        {
            _service = service;
        }

        [HttpPost]
        public ActionResult<UsuarioRequestDTO> Create([FromBody] UsuarioRequestDTO? dto)
        {
            try
            {
                UsuariosModel? model = dto;

                _service.CriaUsuario(model);

                return CreatedAtAction(nameof(GetById), new { id = model.Id }, dto);
            }
            catch (ArgumentNullException)
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public ActionResult<IEnumerable<UsuarioResponseDTO>>? Get()
        {

            return Ok(_service.BuscarTodos()?.Select(u => (UsuarioResponseDTO)u) ?? []);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("{id:int}")]
        public ActionResult<UsuarioResponseDTO> GetById(int id)
        {
            var usuario = (UsuarioResponseDTO)_service.BuscaPorId(id);

            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }

        [Authorize]
        [HttpPut]
        public IActionResult Put(UsuarioRequestDTO dto, int id)
        {
            try
            {
                var model = dto;

                _service.Atualizar(model, id);

                return NoContent();
            }
            catch (ArgumentNullException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _service.Deletar(id);

                return NoContent();
            }
            catch (ArgumentNullException)
            {
                return NotFound();
            }
        }
    }
}
