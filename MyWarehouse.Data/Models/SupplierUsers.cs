namespace MyWarehouse.Data.Models;

public class SupplierUsers
{
    public int IdSupplier { get; set; }
    public Suppliers Supplier { get; set; }
    public int IdUser { get; set; }
    public Users User { get; set; }
}