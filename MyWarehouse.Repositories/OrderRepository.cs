using Microsoft.EntityFrameworkCore;
using MyWarehouse.Data;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;

namespace MyWarehouse.Repositories;

public class OrderRepository : GenericRepository<Orders>, IOrderRepository
{
    public OrderRepository(WarehouseContext context) : base(context)
    {
    }

    //ordini con i dettagli
    public IQueryable<Orders> GetAllOrdersWithDetails()
    {
        return _dbSet
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Include(o => o.Status)
            .AsQueryable();
    }

    //un ordine con i dettagli
    public async Task<Orders?> GetOrderByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Include(od => od.Status)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    //ordini di un utente
    public IQueryable<Orders> GetOrdersByUserId(int userId)
    {
        return _dbSet
            .Where(o => o.IdUser == userId)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Include(o => o.Status)
            .AsQueryable();
    }
}
