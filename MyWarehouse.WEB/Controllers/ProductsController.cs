using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Response;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.Common.Controllers;

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

    //Prodotto per ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var response = await _productService.GetByIdAsync(id);
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }

    //Prodotti di una determinata categoria
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        var response = await _productService.GetByCategoryAsync(categoryId);
        return response.Result ? Ok(response.Data) : NotFound(response.ErrorMessage);
    }

    //Prodotti di un determinato fornitore
    [HttpGet("supplier/{supplierId}")]
    public async Task<IActionResult> GetBySupplier(int supplierId)
    {
        var response = await _productService.GetBySupplierAsync(supplierId);
        return response.Result ? Ok(response.Data) : NotFound(response.ErrorMessage);
    }
    #endregion

    #region POST
    //crea nuovo prodotto
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
            result = response.Result ? Ok(response.Data) : BadRequest(response.ErrorMessage);
        }

        return result;
    }
    #endregion

    #region PUT
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
            result = response.Result ? Ok(response.Result) : BadRequest(response.ErrorMessage);
        }
        return result;
    }
    #endregion

    #region DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        IActionResult result;
        var response = await _productService.DeleteProductAsync(id);
        result = response.Result ? Ok(response.Result) : BadRequest(response.ErrorMessage);
        return result;
    }
    #endregion
}
