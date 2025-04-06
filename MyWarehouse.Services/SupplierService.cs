using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Response;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using MyWarehouse.Interfaces.ServiceInterfaces;
using MyWarehouse.Interfaces.SecurityInterface;

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

    public override async Task<IEnumerable<SupplierDTO>> GetAllAsync()
    {
        var suppliers = await _supplierRepository.GetAllWithCity().ToListAsync();
        return _mapper.Map<IEnumerable<SupplierDTO>>(suppliers);
    }

    public override async Task<ResponseBase<SupplierDTO>> GetByIdAsync(int id)
    {
        var response = new ResponseBase<SupplierDTO>();

        var supplier = await _supplierRepository.GetByIdWithCityAsync(id);

        if (supplier == null)
        {
            response = ResponseBase<SupplierDTO>.Fail("Fornitore non trovato", ErrorCode.NotFound);
        }
        else
        {
            response = ResponseBase<SupplierDTO>.Success(_mapper.Map<SupplierDTO>(supplier));
        }

        return response;
    }

    public async Task<ResponseBase<IEnumerable<SupplierDTO>>> GetSuppliersByCityIdAsync(int cityId)
    {
        var response = new ResponseBase<IEnumerable<SupplierDTO>>();
        try
        {
            var suppliers = await _supplierRepository.GetSuppliersByCity(cityId).ToListAsync();
            if (suppliers == null || !suppliers.Any())
            {
                response = ResponseBase<IEnumerable<SupplierDTO>>.Fail("Nessun fornitore trovato per questa città.", ErrorCode.NotFound);
            }
            else
            {
                response = ResponseBase<IEnumerable<SupplierDTO>>.Success(_mapper.Map<IEnumerable<SupplierDTO>>(suppliers));
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<IEnumerable<SupplierDTO>>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }
        return response;
    }

    //quando crea un fornitore
    //verifica che la città esista
    //che non si ripeta il nome del fornitore
    public async Task<ResponseBase<SupplierDTO>> CreateSupplierAsync(SupplierDTO dto)
    {
        var response = new ResponseBase<SupplierDTO>();
        try
        {
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
                response = ResponseBase<SupplierDTO>.Success(dto);
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<SupplierDTO>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }
        return response;
    }

    //aggiorna il fornitore
    //verifica che esista la città
    //non deve esserci duplicazione del nome
    public async Task<ResponseBase<SupplierDTO>> UpdateSupplierAsync(SupplierDTO dto)
    {
        var response = new ResponseBase<SupplierDTO>();
        var supplier = await _supplierRepository.GetByIdAsync(dto.Id);
        try
        {
            if (supplier == null)
            {
                response = ResponseBase<SupplierDTO>.Fail("Fornitore non trovato", ErrorCode.NotFound);
            }
            else if (!await _authorizationService.CanAccessOwnResourceAsync<Suppliers>(dto.Id, s => s.Id))
            {
                response = ResponseBase<SupplierDTO>.Fail("Non sei autorizzato a modificare questo fornitore.", ErrorCode.Unauthorized);
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
        catch (Exception ex)
        {
            response = ResponseBase<SupplierDTO>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }
        return response;
    }

    //elimina un fornitore
    //impostando la quantità dei suoi prodotti a 0
    public async Task<ResponseBase<bool>> DeleteSupplierAsync(int id)
    {
        var response = new ResponseBase<bool>();
        try
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            if (supplier == null)
            {
                response = ResponseBase<bool>.Fail("Fornitore non trovato", ErrorCode.NotFound);
            }
            else if (!await _authorizationService.CanAccessOwnResourceAsync<Suppliers>(id, s => s.Id))
            {
                response = ResponseBase<bool>.Fail("Non sei autorizzato a eliminare questo fornitore.", ErrorCode.Unauthorized);
            }
            else
            {
                var productsToUpdate = await _supplierRepository.GetProductsBySupplierId(id).ToListAsync();

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
        catch (Exception ex)
        {
            response = ResponseBase<bool>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }
        return response;
    }
}
