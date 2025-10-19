﻿using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuariosService<UsuarioResponseDTO> _service;

        public UsuariosController(IUsuariosService<UsuarioResponseDTO> service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<UsuarioRequestDTO>> Create([FromBody] UsuarioRequestDTO dto)
        {
            try
            {
                await _service.CriarAsync(dto);

                return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
            }
            catch (ArgumentNullException)
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDTO>>>? Get()
        {
            return Ok(await _service.BuscarTodosAsync());
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UsuarioResponseDTO>> GetById(int id)
        {
            var usuario = await _service.BuscarPorIdAsync(id);

            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put([FromRoute] int id,[FromBody] UsuarioRequestDTO dto)
        {
            try
            {
                dto.Id = id;
                await _service.AtualizarAsync(dto);

                return NoContent();
            }
            catch (ArgumentNullException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
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
