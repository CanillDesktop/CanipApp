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
    public class RetiradaEstoqueController : ControllerBase
    {
        private RetiradaEstoqueService _service;

        public RetiradaEstoqueController(RetiradaEstoqueService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RetiradaEstoqueDTO dto)
        {
            try
            {
                RetiradaEstoqueModel model = dto;

                await _service.CriarAsync(model);

                return CreatedAtAction(dto.CodItem, dto);
            }
            catch (ModelIncompletaException ex)
            {
                var erro = new ErrorResponse
                {
                    StatusCode = 400,
                    Title = "Erro ao salvar log de retirada de estoque",
                    Message = ex.Message
                };
                return StatusCode(erro.StatusCode, erro);
            }
        }
    }
}
