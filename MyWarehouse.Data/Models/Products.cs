namespace MyWarehouse.Data.Models;

public class Products : BaseEntity
{
    public int IdCategory { get; set; }
    public int? IdSupplier { get; set; }
    public int Quantity { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    
    public required Categories Category { get; set; }    
    public Suppliers? Supplier { get; set; }
    public ICollection<OrderDetails> OrderDetails { get; set; }
}

