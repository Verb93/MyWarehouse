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
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    #region GET
    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var response = await _categoryService.GetByIdAsync(id);
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }
    #endregion

    #region POST
    [Authorize(Policy = Policies.Admin)]
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryDTO categoryDto)
    {
        IActionResult result;
        ResponseBase<CategoryDTO> response;

        if (categoryDto == null)
        {
            response = ResponseBase<CategoryDTO>.Fail("I dati della categoria non sono validi.", ErrorCode.ValidationError);
            result = BadRequest(response);        
        }
        else
        {
            response = await _categoryService.CreateCategoryAsync(categoryDto);
            result = response.Result ? Ok(response.Data) : BadRequest(response.ErrorMessage);
        }

        return result;
    }
    #endregion

    #region PUT
    [Authorize(Policy = Policies.Admin)]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDTO categoryDto)
    {
        IActionResult result;
        ResponseBase<CategoryDTO> response;
        if (categoryDto == null || categoryDto.Id != id)
        {
            response = ResponseBase<CategoryDTO>.Fail("Dati non validi", ErrorCode.ValidationError);
            result = BadRequest(response);

        }
        else
        {
            response = await _categoryService.UpdateCategoryAsync(categoryDto);
            result = response.Result ? Ok(response.Data) : BadRequest(response.ErrorMessage);
        }

        return result;
    }
    #endregion
}
