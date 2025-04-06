using Microsoft.AspNetCore.Http;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Common.Response;
using System.Security.Claims;

namespace MyWarehouse.Interfaces.ServiceInterfaces;

public interface IUserService : IGenericService<UserDTO>
{
    Task<ResponseBase<bool>> ChangePasswordAsync(ClaimsPrincipal currentUser, int userId, ChangePasswordDTO changePasswordDto);
    Task<ResponseBase<bool>> DeleteUserAsync(ClaimsPrincipal currentUser, int userId);
    Task<ResponseBase<UserDTO>> GetUserByIdAsync(int userId);
    Task<ResponseBase<UserDTO>> UpdateUserAsync(ClaimsPrincipal currentUser, UserDTO userDto);
}
