namespace MyWarehouse.Data.Models;

public class Orders : BaseEntity
{
    public int IdUser { get; set; }
    public int IdStatus { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalPrice { get; set; }
    public int? IdAddress { get; set; }
    public Addresses? Address { get; set; }
    public Users User { get; set; }    
    public StatusOrders Status {  get; set; }
    public ICollection<OrderDetails> OrderDetails { get; set; }
}
