using Microsoft.EntityFrameworkCore;
using MyWarehouse.Data;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;

namespace MyWarehouse.Repositories;

public class CategoryRepository : GenericRepository<Categories>, ICategoryRepository
{
    public CategoryRepository(WarehouseContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbSet.AnyAsync(c => c.Name == name);
    }
}
