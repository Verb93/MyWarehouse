using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Common.Response;
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
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
        IActionResult result;

        if (string.IsNullOrEmpty(userIdFromToken) || userIdFromToken != id.ToString())
        {
            result = Unauthorized(new { message = "Non autorizzato ad accedere a questi dati" });
        }
        else
        {
            var response = await _userService.GetByIdAsync(id);
            result = response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
        }

        return result;
    }
    #endregion

    #region PUT
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDTO userDto)
    {
        var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
        IActionResult result;

        if (string.IsNullOrEmpty(userIdFromToken) || userIdFromToken != id.ToString())
        {
            result = Unauthorized(new { message = "Non autorizzato a modificare questo utente" });
        }
        else
        {
            var response = await _userService.UpdateUserAsync(userDto);
            result = response.Result ? Ok(response.Data) : BadRequest(response.ErrorMessage);
        }

        return result;
    }

    [HttpPut("change-password/{id}")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDTO changePasswordDto)
    {
        var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
        IActionResult result;

        if (string.IsNullOrEmpty(userIdFromToken) || userIdFromToken != id.ToString())
        {
            result = Unauthorized(new { message = "Non autorizzato a modificare la password di questo utente" });
        }
        else
        {
            var response = await _userService.ChangePasswordAsync(id, changePasswordDto);
            result = response.Result ? Ok(new { message = "Password modificata con successo" }) : BadRequest(response.ErrorMessage);
        }

        return result;
    }
    #endregion

    #region DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
        IActionResult result;

        if (string.IsNullOrEmpty(userIdFromToken) || userIdFromToken != id.ToString())
        {
            result = Unauthorized(new { message = "Non autorizzato a eliminare questo utente" });
        }
        else
        {
            var response = await _userService.DeleteUserAsync(id);
            result = response.Result ? Ok(new { message = "Utente eliminato con successo" }) : BadRequest(response.ErrorMessage);
        }

        return result;
    }
    #endregion
}