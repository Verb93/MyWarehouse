using MyWarehouse.Data.Models;

namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface ISupplierRepository : IGenericRepository<Suppliers>
{
    Task<bool> CityExistsAsync(int cityId);
    Task<bool> ExistsByNameAndCityAsync(string name, int cityId);
    IQueryable<Suppliers> GetAllWithCity();
    Task<Suppliers?> GetByIdWithCityAsync(int id);
    IQueryable<Suppliers> GetSuppliersByCity(int cityId);
    Task<List<Suppliers>> GetSuppliersByUserIdAsync(int userId);
    Task<bool> IsSupplierOwnedByUserAsync(int supplierId, int userId);
}
