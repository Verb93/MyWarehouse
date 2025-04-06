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

    public IQueryable<Users> GetAllWithRoles()
    {
        return _dbSet
            .Include(u => u.Role)
            .Where(u => !u.IsDeleted);
    }

    public async Task<Users?> GetByIdWithRoleAsync(int id)
    {
        return await GetAllWithRoles()
            .FirstOrDefaultAsync(u => u.Id == id);
    }


    public async Task<List<RolePermissions>> GetPermissionsByRoleAsync(int roleId)
    {
        return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => rp.IdRole == roleId)
            .ToListAsync();
    }
}
