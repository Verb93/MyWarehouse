using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Common.Response;
using MyWarehouse.Interfaces.SecurityInterface;

namespace MyWarehouse.WEB.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
    {
        ResponseBase<string> response;
        IActionResult result;

        if (loginDto == null)
        {
            response = ResponseBase<string>.Fail("Dati non validi", ErrorCode.ValidationError);
            result = BadRequest(new { message = response.ErrorMessage });
        }
        else
        {
            response = await _authService.LoginAsync(loginDto);

            if (!response.Result)
            {
                result = Unauthorized(new { error = response.ErrorMessage });
            }
            else
            {
                result = Ok(new { token = response.Data });
            }
        }

        return result;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        ResponseBase<UserDTO> response;
        IActionResult result;

        if (dto == null)
        {
            response = ResponseBase<UserDTO>.Fail("Dati non validi", ErrorCode.ValidationError);
            result = BadRequest(new { error = response.ErrorMessage });
        }
        else
        {
            response = await _authService.RegisterAsync(dto);
            result = response.Result ? Ok(response.Data) : BadRequest(new { error = response.ErrorMessage });
        }

        return result;
    }
}

