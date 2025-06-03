namespace MyWarehouse.Data.Models;

public class Permissions : BaseEntity
{
    public string Action { get; set; } = null!;
    public string Description { get; set; } = null!;

    public ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();
}
