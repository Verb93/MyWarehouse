namespace MyWarehouse.Common.DTOs;

public class ProductDTO
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public int IdCategory { get; set; }
    public required string CategoryName { get; set; }
    public int? IdSupplier { get; set; }
    public string SupplierName { get; set; }
}
