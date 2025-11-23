using Shared.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IUsuariosService : IService<UsuarioRequestDTO, UsuarioResponseDTO>
    {
        new Task<IEnumerable<T?>> BuscarTodosAsync();
        new Task<T?> BuscarPorIdAsync(int id);
        new Task<T?> AtualizarAsync(UsuarioRequestDTO dto);
        new Task<T?> CriarAsync(UsuarioRequestDTO dto);
        Task<UsuarioRequestDTO?> ValidarUsuarioAsync(string email, string senha);
        Task SalvarRefreshTokenAsync(int id, string refreshToken, DateTime expira);
        Task<UsuarioResponseDTO?> BuscaPorRefreshTokenAsync(string? refreshToken);
    }
}
