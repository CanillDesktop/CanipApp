using Backend.Models.Produtos;
using Backend.Services;
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
        private readonly ProdutosService _service;

        public ProdutosController(ProdutosService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ProdutosDTO>> Get()
        {
            return Ok(_service.BuscarTodos().Select(p => (ProdutosDTO)p));
        }

        [HttpGet("{id}")]
        public ActionResult<ProdutosDTO> GetById(string id)
        {
            var model = _service.BuscaPorId(id);

            if (model == null)
                return NotFound();


            return Ok((ProdutosDTO)model);
        }

        [HttpPost]
        public IActionResult Create([FromBody] ProdutosDTO dto)
        {
            ProdutosModel model = dto;

            _service.CriaProduto(model);

            return CreatedAtAction(nameof(GetById), new { id = model.IdProduto }, dto);
        }

        [HttpPut("{id}")]
        public IActionResult Put([FromRoute] string id, [FromBody] ProdutosDTO dto)
        {
            try
            {
                _service.Atualizar(id, dto);

                return NoContent();
            }
            catch (ArgumentNullException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                _service.Deletar(id);

            return CreatedAtAction(nameof(GetById), new { id = model.CodigoId }, dto);
        }
    }
}
