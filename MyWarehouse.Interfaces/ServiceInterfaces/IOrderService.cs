using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Response;

namespace MyWarehouse.Interfaces.ServiceInterfaces;
public interface IOrderService : IGenericService<OrderDTO>
{
    Task<ResponseBase<OrderDTO>> CancelOrderAsync(int orderId, int userId, string role);
    Task<ResponseBase<OrderDTO>> CreateOrderAsync(OrderDTO dto);
    Task<ResponseBase<IEnumerable<OrderDTO>>> GetAllOrdersWithDetailsAsync();
    Task<ResponseBase<OrderDTO>> GetOrderByIdWithDetailsAsync(int orderId, int userId, string role);
    Task<ResponseBase<IEnumerable<OrderDTO>>> GetOrdersByUserIdAsync(int userId, int callerUserId, string role);
    Task<ResponseBase<OrderDTO>> UpdateStatusOrderAsync(int orderId, int newStatusId, int userId, string role);
}
