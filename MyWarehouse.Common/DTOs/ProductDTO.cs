namespace MyWarehouse.Common.DTOs;

public class ProductDTO : BaseEntityDTO
{
    public int IdCategory { get; set; }
    public int? IdSupplier { get; set; }
    public int Quantity { get; set; }
    public required string Name { get; set; }    
    public required string CategoryName { get; set; }
    public string SupplierName { get; set; }
    public decimal Price { get; set; }    
}
