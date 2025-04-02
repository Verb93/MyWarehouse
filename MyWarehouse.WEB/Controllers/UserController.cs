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

        if (userIdFromToken != id.ToString())
        {
            return Unauthorized(new { message = "Non autorizzato ad accedere a questi dati" });
        }

        var response = await _userService.GetByIdAsync(id);
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }
    #endregion

    #region PUT
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDTO userDto)
    {
        var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdFromToken != id.ToString())
        {
            return Unauthorized(new { message = "Non puoi modificare i dati di un altro utente" });
        }

        var response = await _userService.UpdateUserAsync(userDto);
        return response.Result ? Ok(response.Data) : BadRequest(response.ErrorMessage);
    }

    [HttpPut("change-password/{id}")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDTO changePasswordDto)
    {
        var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdFromToken != id.ToString())
        {
            return Unauthorized(new { message = "Non puoi modificare la password di un altro utente" });
        }

        var response = await _userService.ChangePasswordAsync(id, changePasswordDto);
        return response.Result ? Ok(new { message = "Password modificata con successo" }) : BadRequest(response.ErrorMessage);
    }
    #endregion

    #region DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdFromToken != id.ToString())
        {
            return Unauthorized(new { message = "Non puoi eliminare un altro utente" });
        }

        var response = await _userService.DeleteUserAsync(id);
        return response.Result ? Ok(new { message = "Utente eliminato con successo" }) : BadRequest(response.ErrorMessage);
    }
    #endregion
}