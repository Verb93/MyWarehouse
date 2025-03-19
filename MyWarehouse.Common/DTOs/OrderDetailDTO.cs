namespace MyWarehouse.Common.DTOs;

public class OrderDetailDTO
{
    public int IdProduct { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}