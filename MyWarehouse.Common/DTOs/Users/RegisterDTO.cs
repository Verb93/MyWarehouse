namespace MyWarehouse.Common.DTOs.Users;

public class RegisterDTO
{
    public string Name { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateOnly BirthDate { get; set; }
}
