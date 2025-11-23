using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Shared.DTOs;
using Shared.Enums;
using Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly string _jwtKey = "chave_simetrica_de_teste_validacao";
    private readonly IUsuariosService _usuariosService;
    private readonly ILogger<LoginController> _logger;

    public LoginController(
        IUsuariosService usuariosService,
        ILogger<LoginController> logger)
    {
        _usuariosService = usuariosService;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint de login - gera AccessToken e RefreshToken
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation($"Tentativa de login para: {request.Login}");

            var usuario = await _usuariosService.ValidarUsuarioAsync(request.Login, request.Senha);

            if (usuario == null)
            {
                _logger.LogWarning($"Login falhou para: {request.Login}");

                return Unauthorized(new ErrorResponse
                {
                    Title = "Acesso não autorizado",
                    StatusCode = 401,
                    Message = "Usuário ou senha inválidos."
                });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Permissao.ToString() ?? PermissoesEnum.LEITURA.ToString()),
                new Claim("UserId", usuario.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "backend",
                audience: "CanilApp",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = Guid.NewGuid().ToString();
        await _usuariosService.SalvarRefreshTokenAsync(usuario.Id, refreshToken, DateTime.Now.AddDays(7));

            UsuarioResponseDTO dto = usuario;
            await _usuariosService.SalvarRefreshTokenAsync(usuario.Id, refreshToken, DateTime.UtcNow.AddDays(7));

            _logger.LogInformation($"✅ Login bem-sucedido para: {usuario.Email}");

            return Ok(new LoginResponseModel
            {
                Token = new TokenResponse
                {
                    AccessToken = tokenString,
                    RefreshToken = refreshToken
                },
            Usuario = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao processar login");

            return StatusCode(500, new ErrorResponse
            {
                Title = "Erro interno",
                StatusCode = 500,
                Message = "Erro ao processar login. Tente novamente."
            });
        }
    }

    /// <summary>
    /// Endpoint de refresh - CORRIGIDO para sempre retornar RefreshToken
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Tentativa de refresh token");

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                _logger.LogWarning("Refresh token vazio ou nulo");
                return BadRequest(new ErrorResponse
                {
                    Title = "Requisição inválida",
                    StatusCode = 400,
                    Message = "RefreshToken é obrigatório"
                });
            }

            var usuario = await _usuariosService.BuscaPorRefreshTokenAsync(request.RefreshToken);

            if (usuario == null)
            {
                _logger.LogWarning("Refresh token inválido ou expirado");

                return Unauthorized(new ErrorResponse
                {
                    Title = "Token inválido",
                    StatusCode = 401,
                    Message = "Refresh token inválido ou expirado."
                });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Permissao.ToString() ?? PermissoesEnum.LEITURA.ToString()),
                new Claim("UserId", usuario.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var novoAccessToken = new JwtSecurityToken(
                issuer: "backend",
                audience: "CanilApp",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            var novoAccessTokenString = new JwtSecurityTokenHandler().WriteToken(novoAccessToken);

            // ============================================================================
            // 🔥 CORREÇÃO CRÍTICA: SEMPRE GERAR E RETORNAR NOVO REFRESHTOKEN
            // ============================================================================
            var novoRefreshToken = Guid.NewGuid().ToString();
            await _usuariosService.SalvarRefreshTokenAsync((int)usuario.Id, novoRefreshToken, DateTime.UtcNow.AddDays(7));

            _logger.LogInformation($"✅ Refresh bem-sucedido para: {usuario.Email}");

            return Ok(new TokenResponse
            {
                AccessToken = novoAccessTokenString,
                RefreshToken = novoRefreshToken // ✅ AGORA SEMPRE RETORNA!
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao processar refresh token");

            return StatusCode(500, new ErrorResponse
            {
                Title = "Erro interno",
                StatusCode = 500,
                Message = "Erro ao processar refresh token. Tente novamente."
            });
        }
    }
}

public record RefreshTokenRequest(string RefreshToken);
public record LoginRequest(string Login, string Senha);