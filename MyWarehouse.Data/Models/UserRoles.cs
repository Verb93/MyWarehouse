namespace MyWarehouse.Data.Models;

public class UserRoles
{
    public int IdUser { get; set; }
    public int IdRole { get; set; }
    public Users User { get; set; }
    public Roles Role { get; set; }
}