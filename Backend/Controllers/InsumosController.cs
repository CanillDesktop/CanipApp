using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;


using Backend.Services;

namespace Backend.Controllers
{
    
    
    [Route("api/[controller]")]
        [ApiController]
    public class InsumosController: ControllerBase

    {
        private readonly IInsumosService _service;
        public InsumosController(IInsumosService service)
                    {
                        _service = service;
                    }

            [HttpGet]
            public async Task<ActionResult<IEnumerable<InsumosDTO>>> Get()
            {
                var insumos = await _service.RetornarInsumo();
                return Ok(insumos);

            }


            [HttpGet("{id:int}")]

            public async Task<ActionResult<InsumosDTO>> GetInsumoById(int id)
            {
                var insumo = await _service.RetornarInsumoId(id);
                if (insumo == null)
                {
                    return NotFound($"Medicamento com o ID {id} não foi encontrado.");
                }
                return Ok(insumo);
            }


            [HttpPost]
            public async Task<ActionResult<InsumosDTO>> Post(InsumosDTO insumoDto)
            {
            var novoMedicamento = await _service.CriarInsumo(insumoDto);
                return CreatedAtAction(nameof(GetInsumoById), new { id = novoMedicamento.CodigoId }, novoMedicamento);
            }


            [HttpPut]

            public async Task<ActionResult<InsumosDTO>> Put(InsumosDTO insumoDto)
            {
            var insumosAtualizado = await _service.AtualizarInsumo(insumoDto);
                if (insumosAtualizado == null)
                {
                    return NotFound($"Medicamento com o ID não foi encontrado.");
                }
                return Ok(insumosAtualizado);
            }


            [HttpDelete("{id:int}")]

            public async Task<ActionResult<MedicamentoDTO>> Delete(int id)
            {
            var sucesso = await _service.DeletarInsumo(id);
                if (!sucesso)
                {
                    return NotFound($"Medicamento com o ID {id} não foi encontrado.");
                }

                return NoContent();
            }
        }
    }



