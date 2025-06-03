using MyWarehouse.Data.Models;

namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface IProductRepository : IGenericRepository<Products>
{
    Task<bool> CategoryExistsAsync(int categoryId);
    Task<bool> SupplierExistsAsync(int? supplierId);
    IQueryable<Products> GetByCategory(int categoryId);
    IQueryable<Products> GetBySupplier(int supplierId);
    IQueryable<Products> GetAllWithDetails();
    Task<Products?> GetByIdWithDetailsAsync(int id);
    Task<bool> IsProductOwnedByUserAsync(int productId, int userId);
    IQueryable<Products> GetOwnedProductsByUser(int userId);
}
