namespace MyWarehouse.Common.DTOs.Orders;

public class OrderDetailDTO : BaseEntityDTO
{
    public int IdProduct { get; set; }
    public int Quantity { get; set; }
    public string? ProductName { get; set; }
    public decimal UnitPrice { get; set; }

}