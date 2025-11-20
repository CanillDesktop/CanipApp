using Backend.Models.Usuarios;

namespace Backend.Repositories.Interfaces
{
    public interface IUsuariosRepository<T> : IRepository<UsuariosModel>
    {
        Task SaveRefreshTokenAsync(int id, string refreshToken, DateTime expira);
        Task<T?> GetRefreshTokenAsync(string? refreshToken);
        Task<T?> GetByEmailAsync(string email);
    }
}
