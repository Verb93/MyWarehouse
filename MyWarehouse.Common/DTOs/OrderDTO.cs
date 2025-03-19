namespace MyWarehouse.Common.DTOs;

public class OrderDTO
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public int IdStatus { get; set; }
    public string StatusDescription { get; set; }
    public int IdUser { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderDetailDTO> OrderDetails { get; set; }
}