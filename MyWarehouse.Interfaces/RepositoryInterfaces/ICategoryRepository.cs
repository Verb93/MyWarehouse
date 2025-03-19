using MyWarehouse.Data.Models;

namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface ICategoryRepository : IGenericRepository<Categories>
{
    Task<bool> ExistsByNameAsync(string name);
}
