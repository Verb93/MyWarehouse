using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Common.Response;
using MyWarehouse.Common.Security;
using MyWarehouse.Interfaces.ServiceInterfaces;
using System.Security.Claims;

namespace MyWarehouse.WEB.Controllers;

[Authorize] 
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    #region GET
    [Authorize(Policy = Policies.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var response = await _userService.GetUserByIdAsync(id);
        return response.Result
            ? Ok(response.Data)
            : StatusCode((int)response.ErrorCode, new { message = response.ErrorMessage });
    }
    #endregion

    #region PUT
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDTO userDto)
    {
        IActionResult result;

        if (userDto == null || userDto.Id != id)
        {
            result = BadRequest(new { message = "Dati non validi" });
        }
        else
        {
            var response = await _userService.UpdateUserAsync(User, userDto);
            result = response.Result ? Ok(response.Data) : StatusCode((int)response.ErrorCode, new { message = response.ErrorMessage });
        }

        return result;
    }

    [HttpPut("change-password/{id}")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDTO changePasswordDto)
    {
        IActionResult result;

        if (changePasswordDto == null)
        {
            result = BadRequest(new { message = "Dati non validi" });
        }
        else
        {
            var response = await _userService.ChangePasswordAsync(User, id, changePasswordDto);
            result = response.Result ? Ok(new { message = "Password modificata con successo!" })
                                     : StatusCode((int)response.ErrorCode, new { message = response.ErrorMessage });
        }

        return result;
    }
    #endregion

    #region DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var response = await _userService.DeleteUserAsync(User, id);

        return response.Result ? Ok(new { message = "Utente eliminato con successo!" })
                                  : StatusCode((int)response.ErrorCode, new { message = response.ErrorMessage });
    }
    #endregion
}