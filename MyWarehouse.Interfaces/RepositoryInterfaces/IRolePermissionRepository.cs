using MyWarehouse.Data.Models;

namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface IRolePermissionRepository : IGenericRepository<RolePermissions>
{
    Task<List<RolePermissions>> GetPermissionsByUserIdAndActionAsync(int userId, string action);
}