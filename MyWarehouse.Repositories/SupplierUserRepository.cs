using MyWarehouse.Data.Models;
using MyWarehouse.Data;
using MyWarehouse.Interfaces.RepositoryInterfaces;

namespace MyWarehouse.Repositories;

public class SupplierUserRepository : ISupplierUserRepository
{
    private readonly WarehouseContext _context;

    public SupplierUserRepository(WarehouseContext context)
    {
        _context = context;
    }

    public async Task AddSupplierUserAsync(int userId, int supplierId)
    {
        var entity = new SupplierUsers
        {
            IdUser = userId,
            IdSupplier = supplierId
        };

        _context.SupplierUsers.Add(entity);
        await _context.SaveChangesAsync();
    }
}