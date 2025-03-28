using System.Data;

namespace MyWarehouse.Data.Models;

public class Users : BaseEntity
{
    public required string Name { get; set; }
    public required string Lastname { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateOnly BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Relazioni
    public int IdRole { get; set; }
    public required Roles Role { get; set; }

    public ICollection<Orders>? Orders { get; set; }
    public ICollection<SupplierUsers> SupplierUsers { get; set; }
}
