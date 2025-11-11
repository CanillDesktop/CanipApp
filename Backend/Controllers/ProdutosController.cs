using Backend.Exceptions;
using Backend.Models.Produtos;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shared.DTOs;
using Shared.Models;
using System.Diagnostics;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutosService _Prodservice;
        private readonly IServiceProvider _serviceProvider;

        public ProdutosController(IProdutosService prodService, IServiceProvider serviceProvider)
        {
            _Prodservice = prodService;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutosDTO>>> Get()
        {
            var produtosLocais = await _Prodservice.BuscarTodosAsync();
            return Ok(produtosLocais);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProdutosDTO>> GetProdutosById(string id)
        {
            var produtos = await _Prodservice.BuscarPorIdAsync(id);
            if (produtos == null)
            {
                return NotFound($"Medicamento com o ID {id} não foi encontrado.");
            }
            return Ok(produtos);
        }

        [HttpPost]
        public async Task<ActionResult<ProdutosDTO>> Post(ProdutosDTO produtoDto)
        {

            var novoProduto = await _Prodservice.CriarAsync(produtoDto);
            try
            {


                using (var scope = _serviceProvider.CreateScope())
                {
                    var syncService = scope.ServiceProvider.GetRequiredService<IProdutosService>();
                   
                }
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"FALHA DE SYNC (Post): {ex.Message}");
            }


            return CreatedAtAction(nameof(GetProdutosById), new { id = novoProduto.IdProduto }, novoProduto);
        }

        [HttpPut]
        public async Task<ActionResult<ProdutosDTO>> Put(ProdutosDTO produtoDto)
        {
            var produtoAtualizado = await _Prodservice.AtualizarAsync(produtoDto);
            if (produtoAtualizado == null)
            {
                return NotFound($"Medicamento com o ID não foi encontrado.");
            }

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var syncService = scope.ServiceProvider.GetRequiredService<IProdutosService>();
                    await _Prodservice.AtualizarAsync(produtoDto);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Falha ao sincronizar (Atualizar): {ex.Message}");
            }

            return Ok(produtoAtualizado);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ProdutosDTO>> Delete(string id)
        {
            var sucesso = await _Prodservice.DeletarAsync(id);
            if (!sucesso)
            {
                return NotFound($"Medicamento com o ID {id} não foi encontrado.");
            }

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var produtosService = scope.ServiceProvider.GetRequiredService<IProdutosService>();
                
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Falha ao sincronizar (Deletar): {ex.Message}");
            }

            return NoContent();
        }
    }
}