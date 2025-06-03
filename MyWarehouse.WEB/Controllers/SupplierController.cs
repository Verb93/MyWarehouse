using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Response;
using MyWarehouse.Common.Security;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.WEB.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SupplierController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    #region GET
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllSupplier()
    {
        var suppliers = await _supplierService.GetAllAsync();
        return Ok(suppliers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSupplierById(int id)
    {
        var response = await _supplierService.GetByIdAsync(id);
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }

    [HttpGet("by-city/{cityId}")]
    public async Task<IActionResult> GetSuppliersByCity(int cityId)
    {
        var response = await _supplierService.GetSuppliersByCityIdAsync(cityId);
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }

    [HttpGet("by-user/{userId}")]
    public async Task<IActionResult> GetSuppliersByUserId(int userId)
    {
        var response = await _supplierService.GetSuppliersByUserIdAsync(userId);
        return response.Result ? Ok(response.Data) : BadRequest(response.ErrorMessage);
    }

    [HttpGet("owned")]
    public async Task<IActionResult> GetOwnedSuppliers()
    {
        var response = await _supplierService.GetOwnedSuppliersAsync();
        return response.Result ? Ok(response.Data) : BadRequest(new { message = response.ErrorMessage });
    }
    #endregion

    #region POST
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] SupplierDTO supplierDto)
    {
        IActionResult result;
        ResponseBase<SupplierDTO> response;

        if (supplierDto == null)
        {
            response = ResponseBase<SupplierDTO>.Fail("Dati non validi", ErrorCode.ValidationError);
            result = BadRequest(response);
        }
        else
        {
            response = await _supplierService.CreateSupplierAsync(supplierDto);
            result = response.Result ? Ok(response.Data) : BadRequest(response.ErrorMessage);
        }
        return result;
    }
    #endregion

    #region PUT
    [Authorize(Policy = Policies.AdminOrSupplier)]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(int id, [FromBody] SupplierDTO supplierDto)
    {
        IActionResult result;
        ResponseBase<SupplierDTO> response;

        if (supplierDto == null || supplierDto.Id != id)
        {
            response = ResponseBase<SupplierDTO>.Fail("Dati non validi", ErrorCode.ValidationError);
            result = BadRequest(response);
        }
        else
        {
            response = await _supplierService.UpdateSupplierAsync(supplierDto);
            result = response.Result ? Ok(response.Data) : BadRequest(response.ErrorMessage);
        }
        return result;
    }
    #endregion

    #region DELETE
    [Authorize(Policy = Policies.AdminOrSupplier)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        IActionResult result;
        var response = await _supplierService.DeleteSupplierAsync(id);
        result = response.Result ? NoContent() : BadRequest(response.ErrorMessage);
        return result;
    }
    #endregion
}
