using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsuariosController : ControllerBase
{
    private readonly ICognitoService _cognitoService;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(ICognitoService cognitoService, ILogger<UsuariosController> logger)
    {
        _cognitoService = cognitoService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioResponseDTO>> Create([FromBody] UsuarioRequestDTO dto)
    {
        try
        {
            var usuario = await _cognitoService.RegisterUserAsync(dto);
            return Ok(usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário");
            return StatusCode(500, new { error = "Erro ao criar usuário" });
        }
    }
}