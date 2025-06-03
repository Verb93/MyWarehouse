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
        return await _dbSet
           .Include(u => u.UserRoles)
           .ThenInclude(ur => ur.Role)
           .SingleOrDefaultAsync(x => x.Email == email);
    }

    public IQueryable<Users> GetAllWithRoles()
    {
        return _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted);
    }

    public async Task<Users?> GetByIdWithRoleAsync(int id)
    {
        return await GetAllWithRoles()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<List<RolePermissions>> GetPermissionsByRolesAsync(List<int> roleIds)
    {
        return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => roleIds.Contains(rp.IdRole))
            .ToListAsync();
    }

    public async Task AddUserRoleAsync(int userId, int roleId)
    {
        var userRole = new UserRoles
        {
            IdUser = userId,
            IdRole = roleId
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();
    }

    public async Task<List<string>> GetUserRoleNamesAsync(int userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.IdUser == userId)
            .Select(ur => ur.Role.Name)
            .ToListAsync();
    }

    public async Task AddUserRolesAsync(int userId, List<int> roleIds)
    {
        var userRoles = roleIds.Distinct().Select(roleId => new UserRoles
        {
            IdUser = userId,
            IdRole = roleId
        });

        _context.UserRoles.AddRange(userRoles);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAllUserRolesAsync(int userId)
    {
        var existingRoles = _context.UserRoles
            .Where(ur => ur.IdUser == userId);

        _context.UserRoles.RemoveRange(existingRoles);
        await _context.SaveChangesAsync();
    }
}
