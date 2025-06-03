namespace MyWarehouse.Common.DTOs;

public class AddressDTO : BaseEntityDTO
{
    public int IdUser { get; set; }
    public string Street { get; set; } = string.Empty;
    public int IdCity { get; set; } 
    public string CityName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}
