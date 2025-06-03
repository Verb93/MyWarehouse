using MyWarehouse.Common.DTOs.Cart;
using MyWarehouse.Common.DTOs.Orders;
using MyWarehouse.Common.Response;

namespace MyWarehouse.Interfaces.ServiceInterfaces;

public interface ICartService
{
    Task<ResponseBase<bool>> AddToCartAsync(AddToCartRequest request);
    Task<ResponseBase<OrderPreviewDTO>> CheckoutAsync(int idAddress);
    Task<ResponseBase<bool>> ClearCartAsync();
    Task<ResponseBase<CartDTO>> GetCartAsync();
    Task<ResponseBase<bool>> RemoveFromCartAsync(int productId);
}
