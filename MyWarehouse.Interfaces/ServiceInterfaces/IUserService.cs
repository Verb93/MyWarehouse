using Microsoft.AspNetCore.Http;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Common.Requests;
using MyWarehouse.Common.Response;
using System.Security.Claims;

namespace MyWarehouse.Interfaces.ServiceInterfaces;

public interface IUserService : IGenericService<UserDTO>
{
    Task<ResponseBase<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest changePasswordDto);
    Task<ResponseBase<bool>> DeleteUserAsync(int userId);
    Task<ResponseBase<UserDTO>> GetCurrentUserAsync();
    Task<ResponseBase<UserDTO>> GetUserByIdAsync(int userId);
    Task<ResponseBase<UserDTO>> UpdateUserAsync(UserDTO userDto);
    Task<ResponseBase<bool>> UpdateUserRolesAsync(int userId, UpdateRolesRequest request);
    Task<ResponseBase<bool>> UpgradeToBusinessAsync(UpgradeToBusinessRequest request);
}
