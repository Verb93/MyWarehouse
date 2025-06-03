using Microsoft.EntityFrameworkCore;
using MyWarehouse.Data;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;

namespace MyWarehouse.Repositories;

public class RoleRepository : GenericRepository<Roles>, IRoleRepository
{
    public RoleRepository(WarehouseContext context) : base(context)
    {
    }
    public async Task<List<Roles>> GetPublicRolesAsync()
    {
        return await _dbSet
            .Where(r => r.Id != 1) // escludi admin
            .ToListAsync();
    }

    public async Task<List<int>> GetRoleIdsByNamesAsync(List<string> roleNames)
    {
        return await _dbSet
            .Where(r => roleNames.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync();
    }

}