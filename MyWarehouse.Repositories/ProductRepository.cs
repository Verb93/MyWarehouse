using Microsoft.EntityFrameworkCore;
using MyWarehouse.Data;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;

namespace MyWarehouse.Repositories;

public class ProductRepository : GenericRepository<Products>, IProductRepository
{
    public ProductRepository(WarehouseContext context) : base(context)
    {
    }

    public async Task<bool> CategoryExistsAsync(int categoryId) //esistenza della categoria
    {
        return await _context.Categories.AnyAsync(c => c.Id == categoryId);
    }

    public async Task<bool> SupplierExistsAsync(int? supplierId) //esistenza del fornitore
    {
        return await _context.Suppliers.AnyAsync(s => s.Id == supplierId);
    }

    public IQueryable<Products> GetByCategory(int categoryId)
    {
        return _dbSet.Where(p => p.IdCategory == categoryId)
                             .Include(p => p.Category)
                             .Include(p => p.Supplier);
    }

    public IQueryable<Products> GetBySupplier(int supplierId)
    {
        return _dbSet.Where(p => p.IdSupplier == supplierId)
                             .Include(p => p.Category)
                             .Include(p => p.Supplier);
    }
    public IQueryable<Products> GetAllWithDetails()
    {
        return _dbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier);
    }

    public async Task<Products?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    //verifica se il prodotto è del fornitore collegato a quell’utente (ownership)
    public async Task<bool> IsProductOwnedByUserAsync(int productId, int userId)
    {
        return await _dbSet
            .Where(p => p.Id == productId && p.IdSupplier != null)
            .AnyAsync(p => _context.SupplierUsers.Any(su =>
                su.IdSupplier == p.IdSupplier &&
                su.IdUser == userId));
    }

    //prendi tutti i prodotti del fornitore collegato a quell’utente (usato nel pannello supplier)
    public IQueryable<Products> GetOwnedProductsByUser(int userId)
    {
        return _dbSet
            .Where(p => p.IdSupplier != null &&
                        _context.SupplierUsers.Any(su =>
                            su.IdSupplier == p.IdSupplier &&
                            su.IdUser == userId))
            .Include(p => p.Category)
            .Include(p => p.Supplier);
    }
}
