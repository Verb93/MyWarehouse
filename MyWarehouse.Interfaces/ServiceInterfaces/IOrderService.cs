using MyWarehouse.Common.DTOs.Orders;
using MyWarehouse.Common.Response;

namespace MyWarehouse.Interfaces.ServiceInterfaces;
public interface IOrderService : IGenericService<OrderDTO>
{
    Task<ResponseBase<OrderDTO>> CancelOrderAsync(int orderId);
    Task<ResponseBase<bool>> CreateOrderAsync(CheckoutRequest dto);
    Task<ResponseBase<IEnumerable<OrderDTO>>> GetAllOrdersWithDetailsAsync();
    Task<ResponseBase<OrderDTO>> GetOrderByIdWithDetailsAsync(int orderId);
    Task<ResponseBase<IEnumerable<OrderDTO>>> GetOrdersBySupplierAsync();
    Task<ResponseBase<IEnumerable<OrderDTO>>> GetOrdersByUserIdAsync(int userId);
    Task<ResponseBase<OrderDTO>> UpdateStatusOrderAsync(int orderId, int newStatusId);
}
