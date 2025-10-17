namespace Backend.Repositories.Interfaces
{
    public interface IRepository<T, U>
    {
        Task<IEnumerable<T>> GetAsync();
        Task<T?> GetByIdAsync(U id);
        Task<T> CreateAsync(T obj);
        Task<T?> UpdateAsync(T obj);
        Task<bool> DeleteAsync(U id);
    }
}
