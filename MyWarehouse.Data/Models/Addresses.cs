using MyWarehouse.Data.Models;

public class Addresses : BaseEntity
{
    public int IdUser { get; set; }
    public Users User { get; set; }
    public string Street { get; set; }
    public bool IsDeleted { get; set; }
    public int IdCity { get; set; }
    public Cities City { get; set; }

}