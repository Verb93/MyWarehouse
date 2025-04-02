namespace MyWarehouse.Data.Models;

public class Roles : BaseEntity
{
    public required string Name { get; set; }
    public ICollection<Users> Users { get; set; }
}
