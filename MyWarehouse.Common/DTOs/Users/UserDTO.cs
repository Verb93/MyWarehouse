namespace MyWarehouse.Common.DTOs.Users;

public class UserDTO : BaseEntityDTO
{
    public required string Name { get; set; }
    public required string Lastname { get; set; }
    public required string Email { get; set; }
    public DateOnly BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public required List<string> Roles { get; set; }
}
