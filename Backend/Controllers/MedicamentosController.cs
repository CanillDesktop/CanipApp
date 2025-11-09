using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using System.Diagnostics;
using System; 
using Microsoft.Extensions.DependencyInjection; 

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicamentosController : ControllerBase
    {
        
        private readonly IMedicamentosService _MedService;
        private readonly IServiceProvider _serviceProvider;


       
        public MedicamentosController(IMedicamentosService medService, IServiceProvider serviceProvider)
        {
            _MedService = medService;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]

        public async Task<ActionResult<IEnumerable<MedicamentoDTO>>> Get()
        {
            var medicamentosLocais = await _MedService.BuscarTodosAsync();
            return Ok(medicamentosLocais);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MedicamentoDTO>> GetMedicamentoById(int id)
        {
            var medicamento = await _MedService.BuscarPorIdAsync(id);
            if (medicamento == null)
            {
                return NotFound($"Medicamento com o ID {id} não foi encontrado.");
            }
            return Ok(medicamento);
        }

        [HttpPost]
        public async Task<ActionResult<MedicamentoDTO>> Post(MedicamentoDTO medicamentoDto)
        {
            
            var novoMedicamento = await _MedService.CriarAsync(medicamentoDto);       
            try
            {
                
               
                using (var scope = _serviceProvider.CreateScope())
                {
                    var syncService = scope.ServiceProvider.GetRequiredService<IMedicamentosService>();
                    await syncService.CriarAsync(novoMedicamento);
                }
            }
            catch (Exception ex)
            {
               
                Debug.WriteLine($"FALHA DE SYNC (Post): {ex.Message}");
            }

           
            return CreatedAtAction(nameof(GetMedicamentoById), new { id = novoMedicamento.CodigoId }, novoMedicamento);
        }

      
        [HttpPut]
        public async Task<ActionResult<MedicamentoDTO>> Put(MedicamentoDTO medicamentoDto)
        {
            var medicamentoAtualizado = await _MedService.AtualizarAsync(medicamentoDto);
            if (medicamentoAtualizado == null)
            {
                return NotFound($"Medicamento com o ID não foi encontrado.");
            }

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var syncService = scope.ServiceProvider.GetRequiredService<IMedicamentosService>();
                    await syncService.AtualizarAsync(medicamentoAtualizado);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Falha ao sincronizar (Atualizar): {ex.Message}");
            }

            return Ok(medicamentoAtualizado);
        }

      
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<MedicamentoDTO>> Delete(int id)
            {
                var sucesso = await _MedService.DeletarAsync(id);
            if (!sucesso)
            {
                return NotFound($"Medicamento com o ID {id} não foi encontrado.");
            }

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var medicamentosService = scope.ServiceProvider.GetRequiredService<IMedicamentosService>();
                    await medicamentosService.DeletarAsync(id);
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

 