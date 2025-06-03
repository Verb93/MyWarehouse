using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Response;

namespace MyWarehouse.Interfaces.ServiceInterfaces;
public interface ISupplierService : IGenericService<SupplierDTO>
{
    Task<ResponseBase<SupplierDTO>> CreateSupplierAsync(SupplierDTO dto);
    Task<ResponseBase<bool>> DeleteSupplierAsync(int id);
    Task<ResponseBase<List<SupplierDTO>>> GetOwnedSuppliersAsync();
    Task<ResponseBase<IEnumerable<SupplierDTO>>> GetSuppliersByCityIdAsync(int cityId);
    Task<ResponseBase<List<SupplierDTO>>> GetSuppliersByUserIdAsync(int userId);
    Task<ResponseBase<SupplierDTO>> UpdateSupplierAsync(SupplierDTO dto);
}
