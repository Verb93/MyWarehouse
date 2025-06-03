using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Response;

namespace MyWarehouse.Interfaces.ServiceInterfaces;

public interface IAddressService : IGenericService<AddressDTO>
{
    Task<ResponseBase<AddressDTO>> AddAddressAsync(AddressDTO dto);
    Task<ResponseBase<bool>> DeleteAddressAsync(int id);
    Task<IEnumerable<AddressDTO>> GetActiveAddressesAsync();
    Task<IEnumerable<AddressDTO>> GetUserAddressesAsync();
    Task<bool> IsAddressOwnedByUserAsync(int id);
    Task<ResponseBase<bool>> RestoreAddressAsync(int id);
    Task<ResponseBase<AddressDTO>> UpdateAddressAsync(AddressDTO dto);
}