using Backend.Models.Medicamentos;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Medicamentos;
using System.Diagnostics;
using System;
using Microsoft.Extensions.DependencyInjection;
using Backend.Repositories.Interfaces;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicamentosController : ControllerBase
    {
        private readonly IMedicamentosService _service;
        private readonly IServiceProvider _serviceProvider;

        public MedicamentosController(IMedicamentosService service, IServiceProvider serviceProvider)
        {
            _service = service;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicamentoCadastroDTO>>> Get([FromQuery] MedicamentosFiltroDTO filtro)
        {
            var filteredRequest = HttpContext.Request.GetDisplayUrl().Contains('?');

            if (filteredRequest)
                return Ok(await _service.BuscarTodosAsync(filtro));
            else
                return Ok(await _service.BuscarTodosAsync());
        }


        [HttpGet("{id:int}")]

        public async Task<ActionResult<MedicamentoCadastroDTO>> GetById(int id)
        {
            var medicamento = await _service.BuscarPorIdAsync(id);
            if (medicamento == null)
            {
                return NotFound($"Medicamento com o ID {id} não foi encontrado.");
            }
            return Ok(medicamento);
        }


        [HttpPost]
        public async Task<ActionResult<MedicamentoCadastroDTO>> Post(MedicamentoCadastroDTO medicamentoDto)
        {
            try
            {
                MedicamentosModel model = medicamentoDto;
                var novoMedicamento = await _service.CriarAsync(model);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var syncService = scope.ServiceProvider.GetRequiredService<IMedicamentosService>();
                }

                return CreatedAtAction(nameof(GetById), new { id = novoMedicamento.IdItem }, novoMedicamento);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FALHA DE SYNC (Post): {ex.Message}");
                return StatusCode(500);
            }
        }


        [HttpPut]

        public async Task<ActionResult<MedicamentoCadastroDTO>> Put(MedicamentoCadastroDTO medicamentoDto)
        {
            try
            {
                var medicamentoAtualizado = await _service.AtualizarAsync(medicamentoDto);
                if (medicamentoAtualizado == null)
                {
                    return NotFound($"Medicamento com o ID não foi encontrado.");
                }

                using (var scope = _serviceProvider.CreateScope())
                {
                    var syncService = scope.ServiceProvider.GetRequiredService<IMedicamentosService>();

                }

                return Ok(medicamentoAtualizado);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Falha ao sincronizar (Atualizar): {ex.Message}");
                return StatusCode(500);
            }

        }


        [HttpDelete("{id:int}")]

        public async Task<ActionResult<MedicamentoCadastroDTO>> Delete(int id)
        {

            try
            {
                var sucesso = await _service.DeletarAsync(id);
                if (!sucesso)
                {
                    return NotFound($"Medicamento com o ID {id} não foi encontrado.");
                }

                using (var scope = _serviceProvider.CreateScope())
                {
                    var medicamentosService = scope.ServiceProvider.GetRequiredService<IMedicamentosService>();

                }

                return NoContent();

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Falha ao sincronizar (Deletar): {ex.Message}");
                return StatusCode(500);
            }
        }
    }
}