using Backend.Models.Produtos;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly ProdutosService _service;

        public ProdutosController(ProdutosService service)
        {
            _service = service;
        }

        [HttpGet]
        public IEnumerable<ProdutosDTO> Get()
        {
            return _service.GetAll().Select(p => (ProdutosDTO)p);
        }

        [HttpGet("{id}")]
        public ActionResult<ProdutosDTO> GetById(string id)
        {
            var model = _service.GetById(id);

            if (model == null)
                return NotFound();


            return (ProdutosDTO)model;
        }

        [HttpPost]
        public IActionResult Create([FromBody] ProdutosDTO dto)
        {
            ProdutosModel model = dto;

            _service.Add(model);

            return CreatedAtAction(nameof(GetById), new { id = model.Codigo }, dto);
        }
    }
}
