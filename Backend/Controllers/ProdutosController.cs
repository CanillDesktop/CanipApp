using Backend.Models.Produtos;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutosService _service;

        public ProdutosController(IProdutosService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutosDTO>>> Get()
        {
            return Ok(await _service.BuscarTodosAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutosDTO>> GetById(string id)
        {
            var model = await _service.BuscarPorIdAsync(id);

            if (model == null)
                return NotFound();


            return Ok(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProdutosDTO dto)
        {
            ProdutosModel model = dto;

            await _service.CriarAsync(model);

            return CreatedAtAction(nameof(GetById), new { id = model.IdProduto }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] string id, [FromBody] ProdutosDTO dto)
        {
            try
            {
                dto.IdProduto = id;
                await _service.AtualizarAsync(dto);

                return NoContent();
            }
            catch (ArgumentNullException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
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
