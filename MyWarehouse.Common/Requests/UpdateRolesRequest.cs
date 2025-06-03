namespace MyWarehouse.Common.Requests;

public class UpdateRolesRequest
{
    public required List<string> Roles { get; set; }
    public int? IdSupplier { get; set; }
}