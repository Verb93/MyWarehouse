using MyWarehouse.Common.DTOs.Users;

namespace MyWarehouse.Interfaces.ServiceInterfaces;

public interface IRoleService : IGenericService<RoleDTO>
{
    Task<List<RoleDTO>> GetPublicRolesAsync();
}
