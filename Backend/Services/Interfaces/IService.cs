namespace Backend.Services.Interfaces
{
    public interface IService<T, U>
    {
        Task<IEnumerable<T>> BuscarTodosAsync();
        Task<T?> BuscarPorIdAsync(U id);
        Task<T?> CriarAsync(T obj);
        Task<T?> AtualizarAsync(T obj);
        Task<bool> DeletarAsync(U id);
    }
}
