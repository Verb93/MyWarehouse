namespace MyWarehouse.Common.DTOs.Users;

public class RegisterDTO
{
    public required string Name { get; set; }
    public required string Lastname { get; set; }
    public required string Email { get; set; }
    public required string Password {get; set;}
    public DateOnly BirthDate { get; set; }
    public int IdRole { get; set; }
    
    // Associa l'utente a un fornitore nella tabella SupplierUsers
    //quindi solo se IdRole == 3
    public int? IdSupplier { get; set; }
}
