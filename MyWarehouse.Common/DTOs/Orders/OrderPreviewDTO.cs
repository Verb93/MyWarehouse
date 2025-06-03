namespace MyWarehouse.Common.DTOs.Orders;

public class OrderPreviewDTO
{
    public string AddressStreet { get; set; }
    public string CityName { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderDetailPreviewDTO> Items { get; set; }
}
