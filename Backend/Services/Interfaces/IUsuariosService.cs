using Shared.DTOs;

namespace Backend.Services.Interfaces;

public interface IUsuariosService<T> where T : class
{
    Task<T?> CriarAsync(UsuarioRequestDTO dto);
    Task<T?> AtualizarAsync(UsuarioRequestDTO dto);
    Task<bool> DeletarAsync(int id);
    Task<IEnumerable<T>> BuscarTodosAsync();
    Task<T?> BuscarPorIdAsync(int id);
    Task<T?> ValidarUsuarioAsync(string login, string senha);
    Task SalvarRefreshTokenAsync(int usuarioId, string refreshToken, DateTime expiry);
    Task<T?> BuscaPorRefreshTokenAsync(string? refreshToken);
}