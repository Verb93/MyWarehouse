namespace MyWarehouse.Common.DTOs.Users;

public class RegisterDTO
{
    public required string Name { get; set; }
    public required string Lastname { get; set; }
    public required string Email { get; set; }
    public required string Password {get; set;}
    public DateTime BirthDate { get; set; }
}
