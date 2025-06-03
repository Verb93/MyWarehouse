using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Data.Models;

namespace MyWarehouse.Common.Security.SecurityInterface;

public interface IJwtService
{
    string GenerateJwtToken(UserDTO userDTO, List<string> roleNames);
}
