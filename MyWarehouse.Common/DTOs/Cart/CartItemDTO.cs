namespace MyWarehouse.Common.DTOs.Cart;

public class CartItemDTO : BaseEntityDTO
{
    public int IdProduct { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
