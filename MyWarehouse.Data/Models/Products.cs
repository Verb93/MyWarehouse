namespace MyWarehouse.Data.Models;

public class Products : BaseEntity
{
    public required string Name { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public int IdCategory { get; set; }
    public required Categories Category { get; set; }
    public int? IdSupplier { get; set; }
    public Suppliers? Supplier { get; set; }
    public ICollection<OrderDetails> OrderDetails { get; set; }
}

