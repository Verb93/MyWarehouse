namespace MyWarehouse.Data.Models;

public class Users : BaseEntity
{
    public required string Name { get; set; }
    public required string Lastname { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Orders>? Orders { get; set; }
    public bool IsDeleted { get; set; } = false;
}

