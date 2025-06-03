using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Response;
using MyWarehouse.Common.Security;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.Common.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    #region GET
    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var response = await _productService.GetByIdAsync(id);
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        IActionResult result;

        var response = await _productService.GetByCategoryAsync(categoryId);

        if (response.Result)
        {
            result = Ok(response.Data);
        }
        else
        {
            result = NotFound(new { message = response.ErrorMessage });
        }

        return result;
    }

    [HttpGet("supplier/{supplierId}")]
    public async Task<IActionResult> GetBySupplier(int supplierId)
    {
        var response = await _productService.GetBySupplierAsync(supplierId);
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }

    [HttpGet("owned")]
    [Authorize(Policy = Policies.Supplier)]
    public async Task<IActionResult> GetOwnedProducts()
    {
        var products = await _productService.GetOwnedProductsAsync();
        return Ok(products);
    }

    #endregion

    #region POST
    // Solo admin e fornitori possono creare prodotti
    [Authorize(Policy = Policies.AdminOrSupplier)]
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductDTO productDto)
    {
        IActionResult result;
        ResponseBase<ProductDTO> response;
        if (productDto == null)
        {
            response = ResponseBase<ProductDTO>.Fail("Dati non validi", ErrorCode.ValidationError);
            result = BadRequest(response);
        }
        else
        {
            response = await _productService.AddProductAsync(productDto);
            result = response.Result ? Ok(response.Data) : BadRequest(new { message = response.ErrorMessage });
        }
        return result;
    }
    #endregion

    #region PUT
    // Solo admin e fornitori possono aggiornare
    [Authorize(Policy = Policies.AdminOrSupplier)]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDTO productDto)
    {
        IActionResult result;
        ResponseBase<ProductDTO> response;
        if (productDto == null || productDto.Id != id)
        {
            response = ResponseBase<ProductDTO>.Fail("Dati non validi", ErrorCode.ValidationError);
            result = BadRequest(response);
        }
        else
        {
            response = await _productService.UpdateProductAsync(productDto);
            result = response.Result ? Ok(response.Data) : BadRequest(new { message = response.ErrorMessage });
        }
        return result;
    }
    #endregion

    #region DELETE
    // Solo admin e fornitori possono eliminare
    [Authorize(Policy = Policies.AdminOrSupplier)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var response = await _productService.DeleteProductAsync(id);
        return response.Result ? Ok(new { message = "Prodotto eliminato correttamente" }) : BadRequest(new { message = response.ErrorMessage });
    }
    #endregion
}
