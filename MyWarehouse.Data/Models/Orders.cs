namespace MyWarehouse.Data.Models;

public class Orders : BaseEntity
{
    public DateTime OrderDate { get; set; }
    public int IdUser { get; set; }
    public required Users User { get; set; }
    public int IdStatus { get; set; }
    public required StatusOrders Status {  get; set; }
    public decimal TotalPrice { get; set; }
    public ICollection<OrderDetails> OrderDetails { get; set; }
}

