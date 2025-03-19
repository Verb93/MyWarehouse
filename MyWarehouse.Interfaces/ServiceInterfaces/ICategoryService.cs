using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Response;

namespace MyWarehouse.Interfaces.ServiceInterfaces;

public interface ICategoryService : IGenericService<CategoryDTO>
{
    Task<ResponseBase<CategoryDTO>> CreateCategoryAsync(CategoryDTO dto);
    Task<ResponseBase<CategoryDTO>> UpdateCategoryAsync(CategoryDTO dto);
}
