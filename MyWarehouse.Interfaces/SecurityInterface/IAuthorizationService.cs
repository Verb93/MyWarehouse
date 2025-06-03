namespace MyWarehouse.Interfaces.SecurityInterface;

public interface IAuthorizationService
{
    int GetCurrentUserId();
    Task<(bool Result, bool OwnOnly)> HasPermissionAsync(int userId, string action);
}