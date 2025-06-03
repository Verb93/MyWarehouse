
namespace MyWarehouse.Common.DTOs.Cart;

public class CartDTO : BaseEntityDTO
{
    public List<CartItemDTO> Items { get; set; }
}
