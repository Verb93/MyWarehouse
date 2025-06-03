using Microsoft.EntityFrameworkCore;
using MyWarehouse.Data;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;

namespace MyWarehouse.Repositories;

public class SupplierRepository : GenericRepository<Suppliers>, ISupplierRepository
{
    public SupplierRepository(WarehouseContext context) : base(context)
    {
    }

    public async Task<bool> CityExistsAsync(int cityId)
    {
        return await _context.Cities.AnyAsync(c => c.Id == cityId);
    }
    public async Task<bool> ExistsByNameAndCityAsync(string name, int cityId)
    {
        return await _dbSet.AnyAsync(s => s.Name == name && s.IdCity == cityId);
    }
    public IQueryable<Suppliers> GetSuppliersByCity(int cityId)
    {
        return _context.Suppliers.Where(s => s.IdCity == cityId).AsQueryable();
    }
    public IQueryable<Suppliers> GetAllWithCity()
    {
        return _dbSet.Include(s => s.City);
    }
    public Task<Suppliers?> GetByIdWithCityAsync(int id)
    {
        return _dbSet.Include(s => s.City)
                     .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Suppliers>> GetSuppliersByUserIdAsync(int userId)
    {
        var supplierIds = await _context.SupplierUsers
            .Where(su => su.IdUser == userId)
            .Select(su => su.IdSupplier)
            .ToListAsync();

        return await _dbSet
            .Where(s => supplierIds.Contains(s.Id))
            .Include(s => s.City)
            .ToListAsync();
    }

    public async Task<bool> IsSupplierOwnedByUserAsync(int supplierId, int userId)
    {
        return await _context.SupplierUsers
            .AnyAsync(su => su.IdSupplier == supplierId && su.IdUser == userId);
    }
}