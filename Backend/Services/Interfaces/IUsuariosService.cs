using Shared.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IUsuariosService : IService<UsuarioRequestDTO, UsuarioResponseDTO>
    {
        Task<UsuarioRequestDTO?> ValidarUsuarioAsync(string email, string senha);
        Task SalvarRefreshTokenAsync(int id, string refreshToken, DateTime expira);
        Task<UsuarioResponseDTO?> BuscaPorRefreshTokenAsync(string? refreshToken);
    }
}
