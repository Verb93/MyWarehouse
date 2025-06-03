using MyWarehouse.Data;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;

namespace MyWarehouse.Repositories;

public class PermissionRepository : GenericRepository<Permissions>, IPermissionRepository
{
    public PermissionRepository(WarehouseContext context) : base(context)
    {
    }
}
