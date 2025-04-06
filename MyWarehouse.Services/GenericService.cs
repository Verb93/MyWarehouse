using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Common.Response;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.Services;


public abstract class GenericService<TEntity, TDTO> : IGenericService<TDTO>
    where TEntity : class
    where TDTO : class
{
    private readonly IGenericRepository<TEntity> _repository;
    private readonly IMapper _mapper;

    public GenericService(IGenericRepository<TEntity> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public virtual async Task<IEnumerable<TDTO>> GetAllAsync()
    {
        var entities = await _repository.GetAll().ToListAsync();
        return _mapper.Map<List<TDTO>>(entities);
    }

    public virtual async Task<ResponseBase<TDTO>> GetByIdAsync(int id)
    {
        var response = new ResponseBase<TDTO>();

        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            response = ResponseBase<TDTO>.Fail("Elemento non trovato", ErrorCode.NotFound);
        }
        else
        {
            response = ResponseBase<TDTO>.Success(_mapper.Map<TDTO>(entity));
        }  

        return response;
    }

    /*public virtual async Task<TDTO> AddAsync(TDTO dto)
    {
        var entity = _mapper.Map<TEntity>(dto);
        var newEntity = await _repository.AddAsync(entity);
        return _mapper.Map<TDTO>(newEntity);
    }*/

    /*public async Task<TDTO> UpdateAsync(TDTO dto)
    {
        var entity = _mapper.Map<TEntity>(dto);
        var updatedEntity = await _repository.UpdateAsync(entity);
        return _mapper.Map<TDTO>(updatedEntity);
    }*/

    /*public virtual async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }*/
}
