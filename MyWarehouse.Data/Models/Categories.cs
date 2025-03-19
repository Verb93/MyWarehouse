namespace MyWarehouse.Data.Models;

public class Categories : BaseEntity
{
    public required string Name { get; set; }
    public ICollection<Products>? Products { get; set; }
}

