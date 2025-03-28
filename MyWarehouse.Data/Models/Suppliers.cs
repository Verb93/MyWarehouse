namespace MyWarehouse.Data.Models;

public class Suppliers : BaseEntity
{
    public required string Name { get; set; }
    public int IdCity { get; set; }
    public required Cities City { get; set; }
    public required ICollection<Products> Products { get; set; }
    public ICollection<SupplierUsers> SupplierUsers { get; set; }
}

