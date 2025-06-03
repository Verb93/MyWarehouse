namespace MyWarehouse.Common.DTOs;

public class SupplierDTO : BaseEntityDTO
{
    public int IdCity { get; set; }
    public required string Name { get; set; }
    public string? CityName { get; set; }
}