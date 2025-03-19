namespace MyWarehouse.Common.DTOs;

public class SupplierDTO
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int IdCity { get; set; }
    public string CityName { get; set; }
}

