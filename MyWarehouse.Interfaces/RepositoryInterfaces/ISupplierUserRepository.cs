namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface ISupplierUserRepository
{
    Task AddSupplierUserAsync(int userId, int supplierId);
}
