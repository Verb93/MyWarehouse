namespace MyWarehouse.Data.Models;

public class RolePermissions : BaseEntity
{
    public int IdRole { get; set; }
    public Roles Role { get; set; } = null!;

    public int IdPermission { get; set; }
    public Permissions Permission { get; set; } = null!;

    public bool OwnOnly { get; set; }
}