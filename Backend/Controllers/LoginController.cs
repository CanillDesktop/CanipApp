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

    private readonly IUsuariosService<UsuarioResponseDTO> _usuariosService;

    public LoginController(IUsuariosService<UsuarioResponseDTO> usuariosService)
    {
        _usuariosService = usuariosService;
    }

    [HttpPost]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        var usuario = await _usuariosService.ValidarUsuarioAsync(request.Login, request.Senha);
        if (usuario == null)
            return Unauthorized(new ErrorResponse
            {
                Title = "Acesso não autorizado",
                StatusCode = 401,
                Message = "Usuário ou senha inválidos."
            });

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Permissao.ToString() ?? PermissoesEnum.LEITURA.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "backend",
            audience: "CanilApp",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Guid.NewGuid().ToString();
        await _usuariosService.SalvarRefreshTokenAsync(usuario.Id, refreshToken, DateTime.Now.AddDays(7));

        UsuarioResponseDTO dto = usuario;

        return Ok(new LoginResponseModel()
        {
            Token = new TokenResponse
            {
                AccessToken = tokenString,
                RefreshToken = refreshToken
            },
            Usuario = dto
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string? refreshToken)
    {
        var usuario = await _usuariosService.BuscaPorRefreshTokenAsync(refreshToken);

        if (usuario == null)
            return Unauthorized("Refresh token inválido ou expirado.");

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Permissao.ToString() ?? PermissoesEnum.LEITURA.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var novoAccessToken = new JwtSecurityToken(
            issuer: "backend",
            audience: "CanilApp",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds
        );

        var novoAccessTokenString = new JwtSecurityTokenHandler().WriteToken(novoAccessToken);

        return Ok(new TokenResponse()
        {
            AccessToken = novoAccessTokenString,
            RefreshToken = refreshToken
        });
    }

}


public record LoginRequest(string Login, string Senha);
