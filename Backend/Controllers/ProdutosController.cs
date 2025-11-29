using Backend.Exceptions;
using Backend.Models.Produtos;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using Backend.Exceptions;
using Shared.Models;
using Shared.DTOs.Produtos;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutosService _service;
        private readonly IServiceProvider _serviceProvider;

        public ProdutosController(IProdutosService service, IServiceProvider serviceProvider)
        {
            _service = service;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutosLeituraDTO>>> Get([FromQuery] ProdutosFiltroDTO filtro)
        {
            var filteredRequest = HttpContext.Request.GetDisplayUrl().Contains('?');

            if (filteredRequest)
                return Ok(await _service.BuscarTodosAsync(filtro));
            else
                return Ok(await _service.BuscarTodosAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutosLeituraDTO>> GetById(int id)
        {
            var model = await _service.BuscarPorIdAsync(id);

            if (model == null)
                return NotFound();


            return Ok(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProdutosCadastroDTO dto)
        {
            try
            {
                ProdutosModel model = dto;
                await _service.CriarAsync(model);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var syncService = scope.ServiceProvider.GetRequiredService<IProdutosService>();
                }

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
            catch (Exception ex)
            {
                Debug.WriteLine($"FALHA DE SYNC (Post): {ex.Message}");
                return StatusCode(500);
            }


            
         
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] ProdutosCadastroDTO dto)
        {
            try
            {
                dto.IdProduto = id;
                await _service.AtualizarAsync(dto);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var syncService = scope.ServiceProvider.GetRequiredService<IProdutosService>();
                }

                return NoContent();
            }
            catch (ArgumentNullException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Falha ao sincronizar (Atualizar): {ex.Message}");
                return StatusCode(500);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var sucesso = await _service.DeletarAsync(id);
                if (!sucesso)
                {
                    return NotFound($"Produto com o ID {id} não foi encontrado.");
                }

                using (var scope = _serviceProvider.CreateScope())
                {
                    var produtosService = scope.ServiceProvider.GetRequiredService<IProdutosService>();
                }

                return NoContent();
            }
            catch (ArgumentNullException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Falha ao sincronizar (Deletar): {ex.Message}");
                return StatusCode(500);
            }
        }
    }
}