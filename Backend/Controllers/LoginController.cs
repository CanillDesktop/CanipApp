using Backend.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Shared.DTOs;
using Shared.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly string _jwtKey = "chave_simetrica_de_teste_validacao";

        private readonly UsuariosService _usuariosService;

        public LoginController(UsuariosService usuariosService)
        {
            _usuariosService = usuariosService;
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var usuario = _usuariosService.ValidaUsuario(request.Email, request.Senha);
            if (usuario == null)
                return Unauthorized("Email ou senha inválidos.");

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, request.Email),
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
            _usuariosService.SalvarRefreshToken(usuario.Id, refreshToken, DateTime.Now.AddDays(7));

            UsuarioResponseDTO dto = usuario;

            return Ok(new { 
                token = tokenString,
                usuario = dto
            });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] string? refreshToken)
        {
            var usuario = _usuariosService.BuscaPorRefreshToken(refreshToken);

            if (usuario == null)
                return Unauthorized("Refresh token inválido ou expirado.");

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, usuario.Email ?? ""),
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

            return Ok(new
            {
                accessToken = novoAccessTokenString,
                refreshToken
            });
        }

    }


    public record LoginRequest(string Email, string Senha);
}
