using Microsoft.EntityFrameworkCore;
using MyWarehouse.Data;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;

namespace MyWarehouse.Repositories;

public class UserRepository : GenericRepository<Users>, IUserRepository
{
    public UserRepository(WarehouseContext context) : base(context)
    {
    }

    public async Task<Users?> GetByEmailAsync(string email)
    {
        return await _dbSet.SingleOrDefaultAsync(x => x.Email == email);
    }
}
