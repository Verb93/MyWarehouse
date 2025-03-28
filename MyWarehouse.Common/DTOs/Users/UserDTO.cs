namespace MyWarehouse.Common.DTOs.Users;

public class UserDTO
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Lastname { get; set; }
    public required string Email { get; set; }
    public DateOnly BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int IdRole { get; set; }
    public required string RoleName { get; set; }
}
