using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Response;
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
        var response = await _productService.GetByCategoryAsync(categoryId);
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }

    [HttpGet("supplier/{supplierId}")]
    public async Task<IActionResult> GetBySupplier(int supplierId)
    {
        var response = await _productService.GetBySupplierAsync(supplierId);
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }
    #endregion

    #region POST
    // Solo admin e fornitori possono creare prodotti
    [Authorize(Roles = "admin,supplier")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductDTO productDto)
    {
        if (productDto == null)
        {
            var error = ResponseBase<ProductDTO>.Fail("Dati non validi", ErrorCode.ValidationError);
            return BadRequest(error);
        }

        var response = await _productService.AddProductAsync(productDto);
        return response.Result ? Ok(response.Data) : BadRequest(new { message = response.ErrorMessage });
    }
    #endregion

    #region PUT
    // Solo admin e fornitori possono aggiornare
    [Authorize(Roles = "admin,supplier")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDTO productDto)
    {
        if (productDto == null || productDto.Id != id)
        {
            var error = ResponseBase<ProductDTO>.Fail("Dati non validi", ErrorCode.ValidationError);
            return BadRequest(error);
        }

        var response = await _productService.UpdateProductAsync(productDto);
        return response.Result ? Ok(response.Data) : BadRequest(new { message = response.ErrorMessage });
    }
    #endregion

    #region DELETE
    // Solo admin e fornitori possono eliminare
    [Authorize(Roles = "admin,supplier")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var response = await _productService.DeleteProductAsync(id);
        return response.Result ? Ok(new { message = "Prodotto eliminato correttamente" }) : BadRequest(new { message = response.ErrorMessage });
    }
    #endregion
}