using MyWarehouse.Data.Models;

namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface IUserRepository : IGenericRepository<Users>
{
    Task AddUserRolesAsync(int userId, List<int> roleIds);
    IQueryable<Users> GetAllWithRoles();
    Task<Users?> GetByEmailAsync(string email);
    Task<Users?> GetByIdWithRoleAsync(int id);
    Task<List<RolePermissions>> GetPermissionsByRolesAsync(List<int> roleIds);
    Task<List<string>> GetUserRoleNamesAsync(int userId);
    Task RemoveAllUserRolesAsync(int userId);
}
//TODO: remove unused