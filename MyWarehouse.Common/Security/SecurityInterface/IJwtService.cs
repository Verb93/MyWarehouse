using MyWarehouse.Common.DTOs.Users;

namespace MyWarehouse.Common.Security.SecurityInterface;

public interface IJwtService
{
    string GenerateJwtToken(UserDTO userDto);
}