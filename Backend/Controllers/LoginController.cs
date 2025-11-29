using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Models;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly ICognitoService _cognitoService;
    private readonly ILogger<LoginController> _logger;

    public LoginController(ICognitoService cognitoService, ILogger<LoginController> logger)
    {
        _cognitoService = cognitoService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation($"Login: {request.Login}");

            var result = await _cognitoService.AuthenticateAsync(request.Login, request.Senha);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ErrorResponse
            {
                Title = "Acesso não autorizado",
                StatusCode = 401,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no login");
            return StatusCode(500, new ErrorResponse
            {
                Title = "Erro interno",
                StatusCode = 500,
                Message = "Erro ao processar login"
            });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new ErrorResponse
                {
                    Title = "Requisição inválida",
                    StatusCode = 400,
                    Message = "RefreshToken obrigatório"
                });
            }

            var result = await _cognitoService.RefreshTokenAsync(request.RefreshToken);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ErrorResponse
            {
                Title = "Token inválido",
                StatusCode = 401,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no refresh");
            return StatusCode(500, new ErrorResponse
            {
                Title = "Erro interno",
                StatusCode = 500,
                Message = "Erro ao renovar token"
            });
        }
    }
}

public record RefreshTokenRequest(string RefreshToken);
public record LoginRequest(string Login, string Senha);