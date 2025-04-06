namespace MyWarehouse.Data.Models;

public class Permissions : BaseEntity
{
    public string Entity { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string HttpVerb { get; set; } = null!;
    public string Endpoint { get; set; } = null!;

    public ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();
}
