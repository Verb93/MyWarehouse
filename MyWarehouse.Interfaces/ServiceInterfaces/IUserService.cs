using Microsoft.AspNetCore.Http;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Common.Response;

namespace MyWarehouse.Interfaces.ServiceInterfaces;

public interface IUserService : IGenericService<UserDTO>
{
    Task<ResponseBase<UserDTO>> RegisterUserAsync(RegisterDTO registerDTO);
    Task<ResponseBase<bool>> AuthenticateUserAsync(LoginDTO loginDTO, HttpContext httpContext);
    ResponseBase<bool> LogOut(HttpContext httpContext);
    Task<ResponseBase<bool>> DeleteUserAsync(int userId);
    Task<ResponseBase<UserDTO>> UpdateUserAsync(UserDTO userDto);
    Task<ResponseBase<bool>> ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDto);
}
