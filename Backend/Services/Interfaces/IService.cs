namespace Backend.Services.Interfaces
{
    public interface IService<T, U>
    {
        Task<IEnumerable<U>> BuscarTodosAsync();
        Task<U?> BuscarPorIdAsync(int id);
        Task<U?> CriarAsync(T obj);
        Task<U?> AtualizarAsync(T obj);
        Task<bool> DeletarAsync(int id);
    }
}
