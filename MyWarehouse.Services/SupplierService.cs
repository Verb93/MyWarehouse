using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Response;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using MyWarehouse.Interfaces.SecurityInterface;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.Services;

public class SupplierService : GenericService<Suppliers, SupplierDTO>, ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductRepository _productRepository;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMapper _mapper;

    public SupplierService(
        ISupplierRepository supplierRepository,
        IProductRepository productRepository,
        IAuthorizationService authorizationService,
        IMapper mapper
    ) : base(supplierRepository, mapper)
    {
        _supplierRepository = supplierRepository;
        _productRepository = productRepository;
        _authorizationService = authorizationService;
        _mapper = mapper;
    }

    // restituisce tutti i fornitori con la città
    public override async Task<IEnumerable<SupplierDTO>> GetAllAsync()
    {
        var suppliers = await _supplierRepository.GetAllWithCity().ToListAsync();
        return _mapper.Map<IEnumerable<SupplierDTO>>(suppliers);
    }

    // restituisce il fornitore per id con la città
    public override async Task<ResponseBase<SupplierDTO>> GetByIdAsync(int id)
    {
        var response = new ResponseBase<SupplierDTO>();
        var supplier = await _supplierRepository.GetByIdWithCityAsync(id);

        response = supplier == null
            ? ResponseBase<SupplierDTO>.Fail("Fornitore non trovato", ErrorCode.NotFound)
            : ResponseBase<SupplierDTO>.Success(_mapper.Map<SupplierDTO>(supplier));

        return response;
    }

    // restituisce i fornitori di una certa città
    public async Task<ResponseBase<IEnumerable<SupplierDTO>>> GetSuppliersByCityIdAsync(int cityId)
    {
        var response = new ResponseBase<IEnumerable<SupplierDTO>>();
        var suppliers = await _supplierRepository.GetSuppliersByCity(cityId).ToListAsync();

        response = suppliers == null || !suppliers.Any()
            ? ResponseBase<IEnumerable<SupplierDTO>>.Fail("Nessun fornitore trovato per questa città.", ErrorCode.NotFound)
            : ResponseBase<IEnumerable<SupplierDTO>>.Success(_mapper.Map<IEnumerable<SupplierDTO>>(suppliers));

        return response;
    }

    public async Task<ResponseBase<List<SupplierDTO>>> GetSuppliersByUserIdAsync(int userId)
    {
        var response = new ResponseBase<List<SupplierDTO>>();

        try
        {
            var suppliers = await _supplierRepository.GetSuppliersByUserIdAsync(userId);
            var supplierDTOs = _mapper.Map<List<SupplierDTO>>(suppliers);
            response = ResponseBase<List<SupplierDTO>>.Success(supplierDTOs);
        }
        catch (Exception ex)
        {
            response = ResponseBase<List<SupplierDTO>>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }

    public async Task<ResponseBase<List<SupplierDTO>>> GetOwnedSuppliersAsync()
    {
        var userId = _authorizationService.GetCurrentUserId();
        return await GetSuppliersByUserIdAsync(userId);
    }

    // crea un nuovo fornitore
    // controlla che esista la città e che non ci sia duplicato di nome+città
    public async Task<ResponseBase<SupplierDTO>> CreateSupplierAsync(SupplierDTO dto)
    {
        var response = new ResponseBase<SupplierDTO>();

        bool cityExists = await _supplierRepository.CityExistsAsync(dto.IdCity);
        bool supplierExists = await _supplierRepository.ExistsByNameAndCityAsync(dto.Name, dto.IdCity);

        if (!cityExists || supplierExists)
        {
            string errorMessage = $"{(cityExists ? "" : $"Città con ID {dto.IdCity} non trovata. ")}" +
                                  $"{(supplierExists ? $"Esiste già un fornitore con il nome '{dto.Name}' nella città selezionata." : "")}";
            response = ResponseBase<SupplierDTO>.Fail(errorMessage, ErrorCode.ValidationError);
        }
        else
        {
            var supplier = _mapper.Map<Suppliers>(dto);
            await _supplierRepository.AddAsync(supplier);
            var supplierWithCity = await _supplierRepository.GetByIdWithCityAsync(supplier.Id);
            var createdSupplier = _mapper.Map<SupplierDTO>(supplier);
            response = ResponseBase<SupplierDTO>.Success(createdSupplier);
        }

        return response;
    }

    // aggiorna il fornitore
    // controlla i permessi, l'ownership, la città e la duplicazione nome+città
    public async Task<ResponseBase<SupplierDTO>> UpdateSupplierAsync(SupplierDTO dto)
    {
        var response = new ResponseBase<SupplierDTO>();
        var userId = _authorizationService.GetCurrentUserId();
        var (hasPermission, ownOnly) = await _authorizationService.HasPermissionAsync(userId, "CanUpdateSupplier");

        if (!hasPermission)
        {
            response = ResponseBase<SupplierDTO>.Fail("Non sei autorizzato a modificare questo fornitore.", ErrorCode.Unauthorized);
        }
        else
        {
            var supplier = await _supplierRepository.GetByIdAsync(dto.Id);

            if (supplier == null)
            {
                response = ResponseBase<SupplierDTO>.Fail("Fornitore non trovato.", ErrorCode.NotFound);
            }
            else if (ownOnly && !await _supplierRepository.IsSupplierOwnedByUserAsync(dto.Id, userId))
            {
                response = ResponseBase<SupplierDTO>.Fail("Non sei il proprietario di questo fornitore.", ErrorCode.Unauthorized);
            }
            else if (supplier.IdCity != dto.IdCity && !await _supplierRepository.CityExistsAsync(dto.IdCity))
            {
                response = ResponseBase<SupplierDTO>.Fail($"Città con ID {dto.IdCity} non trovata.", ErrorCode.NotFound);
            }
            else if (await _supplierRepository.ExistsByNameAndCityAsync(dto.Name, dto.IdCity) && supplier.Name != dto.Name)
            {
                response = ResponseBase<SupplierDTO>.Fail($"Esiste già un fornitore con il nome '{dto.Name}' nella città selezionata.", ErrorCode.ValidationError);
            }
            else
            {
                supplier.Name = dto.Name;
                supplier.IdCity = dto.IdCity;
                var updatedSupplier = await _supplierRepository.UpdateAsync(supplier);
                response = ResponseBase<SupplierDTO>.Success(_mapper.Map<SupplierDTO>(updatedSupplier));
            }
        }
        return response;
    }

    // elimina un fornitore
    // setta a 0 la quantità dei suoi prodotti e scollega i prodotti prima di eliminare
    public async Task<ResponseBase<bool>> DeleteSupplierAsync(int id)
    {
        var response = new ResponseBase<bool>();
        var userId = _authorizationService.GetCurrentUserId();
        var (hasPermission, ownOnly) = await _authorizationService.HasPermissionAsync(userId, "CanDeleteSupplier");

        if (!hasPermission)
        {
            response = ResponseBase<bool>.Fail("Non sei autorizzato a eliminare questo fornitore.", ErrorCode.Unauthorized);
        }
        else
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);

            if (supplier == null)
            {
                response = ResponseBase<bool>.Fail("Fornitore non trovato.", ErrorCode.NotFound);
            }
            else if (ownOnly && !await _supplierRepository.IsSupplierOwnedByUserAsync(id, userId))
            {
                response = ResponseBase<bool>.Fail("Non sei il proprietario di questo fornitore.", ErrorCode.Unauthorized);
            }
            else
            {
                var productsToUpdate = await _productRepository.GetBySupplier(id).ToListAsync();

                if (productsToUpdate.Any())
                {
                    foreach (var product in productsToUpdate)
                    {
                        product.Quantity = 0;
                        product.IdSupplier = null;
                        await _productRepository.UpdateAsync(product);
                    }
                }

                await _supplierRepository.DeleteAsync(id);
                response = ResponseBase<bool>.Success(true);
            }
        }
        return response;
    }
}
