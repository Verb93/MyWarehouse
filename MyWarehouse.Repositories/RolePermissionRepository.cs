using Microsoft.EntityFrameworkCore;
using MyWarehouse.Data;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;

namespace MyWarehouse.Repositories;

public class RolePermissionRepository : GenericRepository<RolePermissions>, IRolePermissionRepository
{
    public RolePermissionRepository(WarehouseContext context) : base(context)
    {
    }

    public async Task<List<RolePermissions>> GetPermissionsByUserIdAndActionAsync(int userId, string action)
    {
        var roleIds = await _context.UserRoles
            .Where(ur => ur.IdUser == userId)
            .Select(ur => ur.IdRole)
            .ToListAsync();

        return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => roleIds.Contains(rp.IdRole) && rp.Permission.Action == action)
            .ToListAsync();
    }

}