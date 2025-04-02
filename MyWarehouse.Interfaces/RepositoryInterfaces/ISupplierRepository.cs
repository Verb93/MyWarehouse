using MyWarehouse.Data.Models;

namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface ISupplierRepository : IGenericRepository<Suppliers>
{
    Task<bool> CityExistsAsync(int cityId);
    Task<bool> ExistsByNameAndCityAsync(string name, int cityId);
    IQueryable<Suppliers> GetAllWithCity();
    Task<Suppliers?> GetByIdWithCityAsync(int id);
    IQueryable<Products> GetProductsBySupplierId(int supplierId);
    IQueryable<Suppliers> GetSuppliersByCity(int cityId);
}
