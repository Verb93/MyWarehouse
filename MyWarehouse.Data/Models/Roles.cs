namespace MyWarehouse.Data.Models;

public class Roles : BaseEntity
{
    public required string Name { get; set; }
    public ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();
    public ICollection<UserRoles> UserRoles { get; set; }
}
