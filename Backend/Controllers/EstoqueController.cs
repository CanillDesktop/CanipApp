using Backend.Exceptions;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Estoque;
using Shared.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EstoqueController : ControllerBase
    {
        private readonly EstoqueItemService _service;

        public EstoqueController(EstoqueItemService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemEstoqueDTO>> GetById(int id)
        {
            var model = await _service.BuscarPorIdAsync(id);

            if (model == null)
                return NotFound();


            return Ok(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ItemEstoqueDTO dto)
        {
            try
            {
                ItemEstoqueModel model = dto;

                await _service.CriarAsync(model);

                return CreatedAtAction(nameof(GetById), new { id = model.IdItem }, dto);
            }
            catch (ModelIncompletaException ex)
            {
                var erro = new ErrorResponse
                {
                    StatusCode = 400,
                    Title = "Erro ao criar produto",
                    Message = ex.Message
                };
                return StatusCode(erro.StatusCode, erro);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] ItemEstoqueDTO dto)
        {
            try
            {
                dto.IdItem = id;
                await _service.AtualizarAsync(dto);

                return NoContent();
            }
            catch (ArgumentNullException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
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
