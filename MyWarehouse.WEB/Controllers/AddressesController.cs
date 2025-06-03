using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Security;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.WEB.Controllers;

[Authorize(Policy = Policies.Client)]
[Route("api/[controller]")]
[ApiController]
public class AddressesController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressesController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    #region GET
    [HttpGet]
    public async Task<IActionResult> GetUserAddresses()
    {
        var addresses = await _addressService.GetUserAddressesAsync();
        return Ok(addresses);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveAddresses()
    {
        var addresses = await _addressService.GetActiveAddressesAsync();
        return Ok(addresses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _addressService.GetByIdAsync(id);
        return response.Result
            ? Ok(response.Data)
            : NotFound(new { message = response.ErrorMessage });
    }
    #endregion

    #region POST
    [HttpPost]
    public async Task<IActionResult> AddAddress([FromBody] AddressDTO dto)
    {
        if (dto == null)
            return BadRequest(new { message = "Dati non validi" });

        var response = await _addressService.AddAddressAsync(dto);
        return response.Result
            ? Ok(response.Data)
            : BadRequest(new { message = response.ErrorMessage });
    }
    #endregion

    #region PUT
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(int id, [FromBody] AddressDTO dto)
    {
        if (dto == null || dto.Id != id)
            return BadRequest(new { message = "Dati non validi" });

        var response = await _addressService.UpdateAddressAsync(dto);
        return response.Result
            ? Ok(response.Data)
            : BadRequest(new { message = response.ErrorMessage });
    }

    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestoreAddress(int id)
    {
        var response = await _addressService.RestoreAddressAsync(id);
        return response.Result
            ? Ok(new { message = "Indirizzo ripristinato con successo" })
            : BadRequest(new { message = response.ErrorMessage });
    }

    #endregion

    #region DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var response = await _addressService.DeleteAddressAsync(id);
        return response.Result
            ? Ok(new { message = "Indirizzo eliminato correttamente" })
            : BadRequest(new { message = response.ErrorMessage });
    }
    #endregion
}