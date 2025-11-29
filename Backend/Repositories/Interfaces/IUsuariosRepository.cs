namespace Backend.Repositories.Interfaces;

public interface IUsuariosRepository<T> : IRepository<T> where T : class
{
    Task<T?> GetByEmailAsync(string email);
    Task<T?> GetByRefreshTokenAsync(string refreshToken);
}