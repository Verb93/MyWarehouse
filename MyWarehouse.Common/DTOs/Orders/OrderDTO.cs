namespace MyWarehouse.Common.DTOs.Orders;

public class OrderDTO : BaseEntityDTO
{
    public int IdUser { get; set; }
    public int IdStatus { get; set; }
    public DateTime OrderDate { get; set; }
    public string? UserEmail { get; set; }
    public string StatusDescription { get; set; }
    public decimal TotalPrice { get; set; }
    public int IdAddress { get; set; }

    public string? AddressStreet { get; set; }
    public string? AddressCityName { get; set; }

    public List<OrderDetailDTO> OrderDetails { get; set; }
}
