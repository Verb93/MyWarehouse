using MyWarehouse.Common.Response;

namespace MyWarehouse.Interfaces.ServiceInterfaces;
public interface IGenericService<TDTO> where TDTO : class
{
    Task<IEnumerable<TDTO>> GetAllAsync();
    Task<ResponseBase<TDTO>> GetByIdAsync(int id);
    //Task<TDTO> AddAsync(TDTO dto);
    //Task<TDTO> UpdateAsync(TDTO dto);
    //Task DeleteAsync(int id);
}
