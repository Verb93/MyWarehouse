using MyWarehouse.Data.Models;
using System.Security.Claims;

namespace MyWarehouse.Interfaces.SecurityInterface;

public interface IAuthorizationService
{
    Task<bool> CanAccessOwnResourceAsync<T>(
        ClaimsPrincipal user,
        int resourceId,
        Func<T, int> getOwnerIdFunc
    ) where T : class;
    Task<bool> CanAccessOwnResourceAsync<T>(int resourceId, Func<T, int> getOwnerIdFunc) where T : class;
    Task<bool> CanCancelOrderAsync(Orders order, int userId, string role);
    Task<bool> CanUpdateOrderStatusAsync(Orders order, int userId, string role);
    Task<bool> CanAccessOrdersByUserAsync(int requestedUserId, int callerUserId, string role);
    Task<bool> CanAccessOrderWithDetailsAsync(Orders order, int userId, string role);
}