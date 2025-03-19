using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.WEB.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CityController : ControllerBase
{
    private readonly ICityService _cityService;

    public CityController(ICityService cityService)
    {
        _cityService = cityService;
    }

    #region GET
    [HttpGet]
    public async Task<IActionResult> GetAllCities()
    {
        var cities = await _cityService.GetAllAsync();
        return Ok(cities);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCityById(int id)
    {
        var response = await _cityService.GetByIdAsync(id);
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }
    #endregion

}
