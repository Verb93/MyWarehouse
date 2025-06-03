using MyWarehouse.Data.Models;

namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface IOrderRepository : IGenericRepository<Orders>
{
    IQueryable<Orders> GetAllOrdersWithDetails();
    Task<Orders?> GetOrderByIdWithDetailsAsync(int id);
    IQueryable<Orders> GetOrdersBySupplier(int userId);
    IQueryable<Orders> GetOrdersByUserId(int userId);
    Task<bool> IsOrderOwnedByUserAsync(int orderId, int userId);
}
