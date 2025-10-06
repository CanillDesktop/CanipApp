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
            return Ok(_service.GetAll().Select(p => (ProdutosDTO)p));
        }

        [HttpGet("{id}")]
        public ActionResult<ProdutosDTO> GetById(string id)
        {
            var model = _service.GetById(id);

            if (model == null)
                return NotFound();


            return Ok((ProdutosDTO)model);
        }

        [HttpPost]
        public IActionResult Create([FromBody] ProdutosDTO dto)
        {
            ProdutosModel model = dto;

            _service.Add(model);

            return CreatedAtAction(nameof(GetById), new { id = model.CodigoId }, dto);
        }
    }
}
