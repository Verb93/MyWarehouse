using MyWarehouse.Data.Models;

namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetAll();
    Task<T> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
