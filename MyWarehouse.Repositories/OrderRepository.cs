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

    // otteniamo tutti gli ordini con i dettagli
    public IQueryable<Orders> GetAllOrdersWithDetails()
    {
        return _dbSet
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Include(o => o.Status)
            .Include(o => o.User)
            .Include(o => o.Address)
            .ThenInclude(a => a.City);
    }

    // otteniamo un ordine per id con i dettagli
    public async Task<Orders?> GetOrderByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(o => o.User)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Include(o => o.Status)
            .Include(o => o.Address)
            .ThenInclude(a => a.City)
            .FirstOrDefaultAsync(o => o.Id == id);

    }

    // otteniamo tutti gli ordini di un utente (per client)
    public IQueryable<Orders> GetOrdersByUserId(int userId)
    {
        return _dbSet
            .Where(o => o.IdUser == userId)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Include(o => o.Status)
            .Include(o => o.User)
            .Include(o => o.Address)
            .ThenInclude(a => a.City);

    }

    // otteniamo tutti gli ordini che contengono i prodotti di un certo supplier (per supplier)
    public IQueryable<Orders> GetOrdersBySupplier(int userId)
    {
        var supplierIds = _context.SupplierUsers
            .Where(su => su.IdUser == userId)
            .Select(su => su.IdSupplier);

        return _dbSet
            .Where(o => o.OrderDetails
                .Any(od => od.Product != null
                        && od.Product.IdSupplier.HasValue
                        && supplierIds.Contains(od.Product.IdSupplier.Value)))
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Include(o => o.Status)
            .Include(o => o.User)
            .Include(o => o.Address)
            .ThenInclude(a => a.City);
    }

    // controlliamo se l'utente è proprietario dell'ordine (come client o come supplier con prodotti nell'ordine)
    public async Task<bool> IsOrderOwnedByUserAsync(int orderId, int userId)
    {
        var order = await _dbSet
            .Where(o => o.Id == orderId)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return false;
        }

        if (order.IdUser == userId)
        {
            return true;
        }

        var supplierIds = await _context.SupplierUsers
            .Where(su => su.IdUser == userId)
            .Select(su => su.IdSupplier)
            .ToListAsync();

        return order.OrderDetails
            .Any(od => od.Product != null
                    && od.Product.IdSupplier.HasValue
                    && supplierIds.Contains(od.Product.IdSupplier.Value));
    }
}
