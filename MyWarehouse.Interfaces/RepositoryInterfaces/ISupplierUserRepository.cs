

namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface ISupplierUserRepository
{
    Task AddSupplierUserAsync(int userId, int supplierId);
    Task<bool> ExistsAsync(int userId, int supplierId);
    Task<List<int>> GetSupplierIdsByUserIdAsync(int userId);
    Task<List<int>> GetUserIdsBySupplierIdAsync(int supplierId);
    Task RemoveAllSupplierUsersByUserIdAsync(int userId);
    Task RemoveSupplierUserAsync(int userId, int supplierId);
}

//TODO: remove unused
