using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Response;

namespace MyWarehouse.Interfaces.ServiceInterfaces;
public interface IProductService : IGenericService<ProductDTO>
{
    Task<ResponseBase<ProductDTO>> AddProductAsync(ProductDTO dto);
    Task<ResponseBase<bool>> DeleteProductAsync(int productId);
    Task<ResponseBase<IEnumerable<ProductDTO>>> GetByCategoryAsync(int categoryId);
    Task<ResponseBase<IEnumerable<ProductDTO>>> GetBySupplierAsync(int supplierId);
    Task<IEnumerable<ProductDTO>> GetOwnedProductsAsync();
    Task<ResponseBase<ProductDTO>> UpdateProductAsync(ProductDTO dto);
}
