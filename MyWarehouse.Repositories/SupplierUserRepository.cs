using MyWarehouse.Data.Models;
using MyWarehouse.Data;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task<int?> GetSupplierIdByUserIdAsync(int userId)
    {
        return await _context.SupplierUsers
            .Where(su => su.IdUser == userId)
            .Select(su => (int?)su.IdSupplier)
            .FirstOrDefaultAsync();
    }

    public async Task<List<int>> GetSupplierIdsByUserId(int userId)
    {
        return await _context.SupplierUsers
            .Where(su => su.IdUser == userId)
            .Select(su => su.IdSupplier)
            .ToListAsync();
    }
}