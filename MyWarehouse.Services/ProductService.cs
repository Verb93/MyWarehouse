using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Response;
using MyWarehouse.Common.Security;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.Services;

public class ProductService : GenericService<Products, ProductDTO>, IProductService
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository repository,
        IMapper mapper
    ) : base(repository, mapper)
    {
        _repository = repository;
        _mapper = mapper;
        
    }

    public override async Task<IEnumerable<ProductDTO>> GetAllAsync()
    {
        var products = await _repository.GetAllWithDetails().ToListAsync();
        return _mapper.Map<IEnumerable<ProductDTO>>(products);
    }

    public override async Task<ResponseBase<ProductDTO>> GetByIdAsync(int id)
    {
        var response = new ResponseBase<ProductDTO>();
        var product = await _repository.GetByIdWithDetailsAsync(id);

        if (product == null)
        {
            response = ResponseBase<ProductDTO>.Fail("Prodotto non trovato", ErrorCode.NotFound);
        }
        else
        {
            response = ResponseBase<ProductDTO>.Success(_mapper.Map<ProductDTO>(product));
        }

        return response;
    }

    //all'inserimento di un nuovo prodotto controllo
    //che idcategory e idsupplier siano validi
    //che la quantità non sia negativa
    //che il prezzo non sia negativo
    public async Task<ResponseBase<ProductDTO>> AddProductAsync(ProductDTO dto)
    {
        var response = new ResponseBase<ProductDTO>();
        bool categoryExists = await _repository.CategoryExistsAsync(dto.IdCategory);
        bool supplierExists = await _repository.SupplierExistsAsync(dto.IdSupplier);
        try
        {
            if (!categoryExists || !supplierExists)
            {
                string errorMessage = $"{(categoryExists ? "" : $"Categoria con ID {dto.IdCategory} non trovata. ")}" +
                                      $"{(supplierExists ? "" : $"Fornitore con ID {dto.IdSupplier} non trovato.")}";
                response = ResponseBase<ProductDTO>.Fail(errorMessage, ErrorCode.NotFound);
            }
            else if (dto.Quantity <= 0)
            {
                response = ResponseBase<ProductDTO>.Fail("La quantità iniziale non può essere 0 o negativa.", ErrorCode.ValidationError);
            }
            else if (dto.Price <= 0)
            {
                response = ResponseBase<ProductDTO>.Fail("Il prezzo non può essere 0 o negativo.", ErrorCode.ValidationError);
            }
            else
            {
                var product = _mapper.Map<Products>(dto);
                await _repository.AddAsync(product);
                response = ResponseBase<ProductDTO>.Success(dto);
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<ProductDTO>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }

    //per modificare un prodotto posso:
    //modificarne la quantità, che può essere 0 ma non negativa
    //modificarne il prezzo, che non può essere negativo
    //modificarne il nome
    public async Task<ResponseBase<ProductDTO>> UpdateProductAsync(ProductDTO dto)
    {
        var response = new ResponseBase<ProductDTO>();
        var product = await _repository.GetByIdAsync(dto.Id);
        try
        {
            if (product == null)
            {
                response = ResponseBase<ProductDTO>.Fail("prodotto non trovato", ErrorCode.NotFound);
            }
            else if (dto.Quantity < 0)
            {
                response = ResponseBase<ProductDTO>.Fail("La quantità non può essere negativa.", ErrorCode.ValidationError);
            }
            else if (dto.Price < 0)
            {
                response = ResponseBase<ProductDTO>.Fail("Il prezzo non può essere negativo", ErrorCode.ValidationError);
            }
            else
            {
                product.Name = dto.Name ?? product.Name;
                product.Quantity = dto.Quantity;
                product.Price = dto.Price;

                var updatedProduct = await _repository.UpdateAsync(product);
                response = ResponseBase<ProductDTO>.Success(_mapper.Map<ProductDTO>(updatedProduct));
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<ProductDTO>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }

    // setta la quantità del prodotto a 0 quando viene eliminato
    // quindi il prodotto in realtà non viene eliminato
    public async Task<ResponseBase<bool>> DeleteProductAsync(int productId)
    {
        var response = new ResponseBase<bool>();
        var product = await _repository.GetByIdAsync(productId);

        try
        {
            if (product == null)
            {
                response = ResponseBase<bool>.Fail("Prodotto non trovato", ErrorCode.NotFound);
            }
            else
            {
                product.Quantity = 0;
                await _repository.UpdateAsync(product);
                response = ResponseBase<bool>.Success(true);
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<bool>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }
        return response;
    }

    //otteniamo tutti i prodotti di una categoria
    //messaggio se non ci sono
    public async Task<ResponseBase<IEnumerable<ProductDTO>>> GetByCategoryAsync(int categoryId)
    {
        var response = new ResponseBase<IEnumerable<ProductDTO>>();
        var products = await _repository.GetByCategory(categoryId).ToListAsync();

        try
        {
            if (products == null || !products.Any())
            {
                response = ResponseBase<IEnumerable<ProductDTO>>.Fail("Nessun prodotto trovato per questa categoria.", ErrorCode.NotFound);
            }
            else
            {
                response = ResponseBase<IEnumerable<ProductDTO>>.Success(_mapper.Map<IEnumerable<ProductDTO>>(products));
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<IEnumerable<ProductDTO>>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }

    //otteniamo tutti i prodotti di un fornitore
    //messaggio se non ci sono
    public async Task<ResponseBase<IEnumerable<ProductDTO>>> GetBySupplierAsync(int supplierId)
    {
        var response = new ResponseBase<IEnumerable<ProductDTO>>();
        var products = await _repository.GetBySupplier(supplierId).ToListAsync();

        try
        {
            if (products == null || !products.Any())
            {
                response = ResponseBase<IEnumerable<ProductDTO>>.Fail("Nessun prodotto trovato per questo fornitore.", ErrorCode.NotFound);
            }
            else
            {
                response = ResponseBase<IEnumerable<ProductDTO>>.Success(_mapper.Map<IEnumerable<ProductDTO>>(products));
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<IEnumerable<ProductDTO>>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }
}
