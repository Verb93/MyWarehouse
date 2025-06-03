using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Response;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using MyWarehouse.Interfaces.SecurityInterface;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.Services;

public class AddressService : GenericService<Addresses, AddressDTO>, IAddressService
{
    private readonly IAddressRepository _repository;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMapper _mapper;

    public AddressService(
        IAddressRepository repository,
        IAuthorizationService authorizationService,
        IMapper mapper
    ) : base(repository, mapper)
    {
        _repository = repository;
        _authorizationService = authorizationService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AddressDTO>> GetUserAddressesAsync()
    {
        var userId = _authorizationService.GetCurrentUserId();
        var addresses = await _repository.GetAllAddressesByUserId(userId).ToListAsync();
        return _mapper.Map<IEnumerable<AddressDTO>>(addresses);
    }

    //Per il checkout:solo quelli attivi
    public async Task<IEnumerable<AddressDTO>> GetActiveAddressesAsync()
    {
        var userId = _authorizationService.GetCurrentUserId();
        var addresses = await _repository.GetActiveAddressesByUserId(userId).ToListAsync();
        return _mapper.Map<IEnumerable<AddressDTO>>(addresses);
    }

    public async Task<ResponseBase<AddressDTO>> AddAddressAsync(AddressDTO dto)
    {
        var response = new ResponseBase<AddressDTO>();
        var userId = _authorizationService.GetCurrentUserId();

        dto.IdUser = userId;

        var address = _mapper.Map<Addresses>(dto);
        await _repository.AddAsync(address);

        response = ResponseBase<AddressDTO>.Success(dto);
        return response;
    }

    public async Task<ResponseBase<bool>> DeleteAddressAsync(int id)
    {
        var response = new ResponseBase<bool>();
        var userId = _authorizationService.GetCurrentUserId();

        var isOwner = await _repository.IsAddressOwnedByUserAsync(id, userId);
        var address = await _repository.GetByIdAsync(id);

        if (!isOwner)
        {
            response = ResponseBase<bool>.Fail("Non autorizzato ad eliminare questo indirizzo.", ErrorCode.Unauthorized);
        }
        else if (address == null)
        {
            response = ResponseBase<bool>.Fail("Indirizzo non trovato.", ErrorCode.NotFound);
        }
        else
        {
            address.IsDeleted = true;
            await _repository.UpdateAsync(address);
            response = ResponseBase<bool>.Success(true);
        }

        return response;
    }

    public async Task<ResponseBase<bool>> RestoreAddressAsync(int id)
    {
        var response = new ResponseBase<bool>();
        var userId = _authorizationService.GetCurrentUserId();

        var isOwner = await _repository.IsAddressOwnedByUserAsync(id, userId);
        var address = await _repository.GetByIdAsync(id);

        if (!isOwner)
        {
            response = ResponseBase<bool>.Fail("Non autorizzato al ripristino.", ErrorCode.Unauthorized);
        }
        else if (address == null)
        {
            response = ResponseBase<bool>.Fail("Indirizzo non trovato.", ErrorCode.NotFound);
        }
        else
        {
            address.IsDeleted = false;
            await _repository.UpdateAsync(address);
            response = ResponseBase<bool>.Success(true);
        }

        return response;
    }

    public async Task<ResponseBase<AddressDTO>> UpdateAddressAsync(AddressDTO dto)
    {
        var response = new ResponseBase<AddressDTO>();
        var userId = _authorizationService.GetCurrentUserId();

        var isOwner = await _repository.IsAddressOwnedByUserAsync(dto.Id, userId);
        var entity = await _repository.GetByIdAsync(dto.Id);

        if (!isOwner)
        {
            response = ResponseBase<AddressDTO>.Fail("Non autorizzato a modificare questo indirizzo.", ErrorCode.Unauthorized);
        }
        else if (entity == null)
        {
            response = ResponseBase<AddressDTO>.Fail("Indirizzo non trovato.", ErrorCode.NotFound);
        }
        else
        {
            entity.Street = dto.Street;
            entity.IdCity = dto.IdCity;

            var updated = await _repository.UpdateAsync(entity);
            response = ResponseBase<AddressDTO>.Success(_mapper.Map<AddressDTO>(updated));
        }

        return response;
    }

    public async Task<bool> IsAddressOwnedByUserAsync(int id)
    {
        var userId = _authorizationService.GetCurrentUserId();
        return await _repository.IsAddressOwnedByUserAsync(id, userId);
    }
}
