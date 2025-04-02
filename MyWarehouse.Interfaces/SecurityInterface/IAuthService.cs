using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Common.Response;

namespace MyWarehouse.Interfaces.SecurityInterface;

public interface IAuthService
{
    Task<ResponseBase<string>> LoginAsync(LoginDTO loginDTO);
    Task<ResponseBase<UserDTO>> RegisterAsync(RegisterDTO registerDTO);
}
