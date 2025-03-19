using MyWarehouse.Data.Models;

namespace MyWarehouse.Interfaces.RepositoryInterfaces;

public interface IOrderRepository : IGenericRepository<Orders>
{
    IQueryable<Orders> GetAllOrdersWithDetails();
    Task<Orders?> GetOrderByIdWithDetailsAsync(int id);
    IQueryable<Orders> GetOrdersByUserId(int userId);
}
