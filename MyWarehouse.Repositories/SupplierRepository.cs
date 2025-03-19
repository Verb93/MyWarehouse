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
    public IQueryable<Products> GetProductsBySupplierId(int supplierId)
    {
        return _context.Products.Where(p => p.IdSupplier == supplierId).AsQueryable();
    }

    public IQueryable<Suppliers> GetSuppliersByCity(int cityId)
    {
        return _context.Suppliers.Where(s => s.IdCity == cityId).AsQueryable();
    }

}