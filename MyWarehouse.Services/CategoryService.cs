using AutoMapper;
using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Response;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using MyWarehouse.Interfaces.ServiceInterfaces;


namespace MyWarehouse.Services;

public class CategoryService : GenericService<Categories, CategoryDTO>, ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;
    public CategoryService(ICategoryRepository repository, IMapper mapper) : base(repository, mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    //nel momento della creazione di una categoria
    //controlliamo se esiste già quel nome
    //controlliamo che la stringa non sia vuota
    public async Task<ResponseBase<CategoryDTO>> CreateCategoryAsync(CategoryDTO dto)
    {
        var response = new ResponseBase<CategoryDTO>();

        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                response = ResponseBase<CategoryDTO>.Fail("Il nome della categoria non può essere vuoto.", ErrorCode.ValidationError);
            }
            else if (await _repository.ExistsByNameAsync(dto.Name))
            {
                response = ResponseBase<CategoryDTO>.Fail($"Esiste già una categoria con nome '{dto.Name}'", ErrorCode.ValidationError);
            }
            else
            {
                var category = _mapper.Map<Categories>(dto);
                var createdCategory = await _repository.AddAsync(category);
                response = ResponseBase<CategoryDTO>.Success(_mapper.Map<CategoryDTO>(createdCategory));
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<CategoryDTO>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }

    //nel momento della modifica
    //controlliamo se esiste già quel nome
    public async Task<ResponseBase<CategoryDTO>> UpdateCategoryAsync(CategoryDTO dto)
    {
        var response = new ResponseBase<CategoryDTO>();

        try
        {
            var category = await _repository.GetByIdAsync(dto.Id);

            if (category == null)
            {
                response = ResponseBase<CategoryDTO>.Fail("Categoria non trovata", ErrorCode.NotFound);
            }
            else if (string.IsNullOrWhiteSpace(dto.Name))
            {
                response = ResponseBase<CategoryDTO>.Fail("Il nome della categoria non può essere vuoto.", ErrorCode.ValidationError);
            }
            else if (category.Name != dto.Name && await _repository.ExistsByNameAsync(dto.Name))
            {
                response = ResponseBase<CategoryDTO>.Fail($"Il nome '{dto.Name}' è già in uso", ErrorCode.ValidationError);
            }
            else
            {
                category.Name = dto.Name;
                var updatedCategory = await _repository.UpdateAsync(category);
                response = ResponseBase<CategoryDTO>.Success(_mapper.Map<CategoryDTO>(updatedCategory));
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<CategoryDTO>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }
}
