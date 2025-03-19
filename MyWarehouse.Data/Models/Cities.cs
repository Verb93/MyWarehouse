namespace MyWarehouse.Data.Models;

public class Cities : BaseEntity
{
    public required string Name { get; set; }
    public required ICollection<Suppliers> Suppliers { get; set; }
}

