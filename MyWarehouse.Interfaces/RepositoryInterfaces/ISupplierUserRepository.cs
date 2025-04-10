﻿
namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface ISupplierUserRepository
{
    Task AddSupplierUserAsync(int userId, int supplierId);
    Task<int?> GetSupplierIdByUserIdAsync(int userId);
    Task<List<int>> GetSupplierIdsByUserId(int userId);
}
