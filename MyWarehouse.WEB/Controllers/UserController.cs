using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Common.Response;
using MyWarehouse.Interfaces.ServiceInterfaces;
using System.Security.Claims;

namespace MyWarehouse.WEB.Controllers;

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

        if (string.IsNullOrEmpty(userIdFromToken) || userIdFromToken != id.ToString())
        {
            return Unauthorized(new { message = "Non autorizzato ad accedere a questi dati" });
        }

        var response = await _userService.GetByIdAsync(id);
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }
    #endregion

    #region POST
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
    {
        IActionResult result;
        ResponseBase<UserDTO> response;

        if (registerDto == null)
        {
            response = ResponseBase<UserDTO>.Fail("Dati non validi", ErrorCode.ValidationError);
            result = BadRequest(response);
        }
        else
        {
            response = await _userService.RegisterUserAsync(registerDto);
            result = response.Result ? Ok(response.Data) : BadRequest(response.ErrorMessage);
        }

        return result;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
    {
        IActionResult result;
        ResponseBase<string> response;

        if (loginDto == null)
        {
            response = ResponseBase<string>.Fail("Dati non validi", ErrorCode.ValidationError);
            result = BadRequest(new { message = response.ErrorMessage });
        }
        else
        {
            response = await _userService.AuthenticateUserAsync(loginDto);
            result = response.Result ? Ok(new { message = "Login effettuato con successo", token = response.Data }) : Unauthorized(new { message = response.ErrorMessage });
        }

        return result;
    }
    #endregion

    #region PUT
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDTO userDto)
    {
        IActionResult result;
        ResponseBase<UserDTO> response;

        var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdFromToken) || userIdFromToken != id.ToString())
        {
            result = Unauthorized(new { message = "Non puoi modificare i dati di un altro utente" });
        }
        else
        {
            response = await _userService.UpdateUserAsync(userDto);
            result = response.Result ? Ok(response.Data) : BadRequest(response.ErrorMessage);
        }

        return result;
    }

    [HttpPut("change-password/{id}")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDTO changePasswordDto)
    {
        IActionResult result;
        ResponseBase<bool> response;

        var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdFromToken) || userIdFromToken != id.ToString())
        {
            result = Unauthorized(new { message = "Non puoi modificare la password di un altro utente" });
        }
        else
        {
            response = await _userService.ChangePasswordAsync(id, changePasswordDto);
            result = response.Result ? Ok(new { message = "Password modificata con successo" }) : BadRequest(response.ErrorMessage);
        }

        return result;
    }
    #endregion

    #region DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        IActionResult result;
        ResponseBase<bool> response;

        var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdFromToken) || userIdFromToken != id.ToString())
        {
            result = Unauthorized(new { message = "Non puoi eliminare un altro utente" });
        }
        else
        {
            response = await _userService.DeleteUserAsync(id);
            result = response.Result ? Ok(new { message = "Utente eliminato con successo" }) : BadRequest(response.ErrorMessage);
        }

        return result;
    }
    #endregion
}
