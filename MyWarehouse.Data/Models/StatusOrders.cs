namespace MyWarehouse.Data.Models;

public class StatusOrders : BaseEntity
{
    public required string Description { get; set; }
    public ICollection<Orders> Orders { get; set; }
}