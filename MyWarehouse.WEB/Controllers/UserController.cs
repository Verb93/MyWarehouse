using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Common.Requests;
using MyWarehouse.Common.Security;
using MyWarehouse.Interfaces.ServiceInterfaces;

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

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var response = await _userService.GetCurrentUserAsync();

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
            var response = await _userService.UpdateUserAsync(userDto);
            result = response.Result ? Ok(response.Data) : StatusCode((int)response.ErrorCode, new { message = response.ErrorMessage });
        }

        return result;
    }

    [Authorize(Policy = Policies.Admin)]
    [HttpPut("{id}/roles")]
    public async Task<IActionResult> UpdateUserRoles(int id, [FromBody] UpdateRolesRequest request)
    {
        IActionResult result;

        if (request == null || request.Roles == null || !request.Roles.Any())
        {
            result = BadRequest(new { message = "Richiesta non valida: ruoli mancanti." });
        }
        else
        {
            var response = await _userService.UpdateUserRolesAsync(id, request);
            result = response.Result
                ? Ok(new { message = "Ruoli aggiornati con successo." })
                : StatusCode((int)response.ErrorCode, new { message = response.ErrorMessage });
        }

        return result;
    }


    [HttpPut("change-password/{id}")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest changePasswordDto)
    {
        IActionResult result;

        if (changePasswordDto == null)
        {
            result = BadRequest(new { message = "Dati non validi" });
        }
        else
        {
            var response = await _userService.ChangePasswordAsync(id, changePasswordDto);
            result = response.Result ? Ok(new { message = "Password modificata con successo!" })
                                     : StatusCode((int)response.ErrorCode, new { message = response.ErrorMessage });
        }

        return result;
    }
    #endregion

    [Authorize]
    [HttpPost("upgrade-to-business")]
    public async Task<IActionResult> UpgradeToBusiness([FromBody] UpgradeToBusinessRequest request)
    {
        var response = await _userService.UpgradeToBusinessAsync(request);

        return response.Result
            ? Ok(new { message = "Upgrade completato." })
            : BadRequest(new { error = response.ErrorMessage });
    }


    #region DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var response = await _userService.DeleteUserAsync(id);

        return response.Result ? Ok(new { message = "Utente eliminato con successo!" })
                                  : StatusCode((int)response.ErrorCode, new { message = response.ErrorMessage });
    }
    #endregion
}