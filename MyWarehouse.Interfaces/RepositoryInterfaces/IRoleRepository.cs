using MyWarehouse.Data.Models;

namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface IRoleRepository : IGenericRepository<Roles>
{
    Task<List<Roles>> GetPublicRolesAsync();
    Task<List<int>> GetRoleIdsByNamesAsync(List<string> roleNames);
}