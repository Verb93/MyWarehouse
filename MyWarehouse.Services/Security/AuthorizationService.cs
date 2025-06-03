using Microsoft.AspNetCore.Http;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using MyWarehouse.Interfaces.SecurityInterface;
using System.Security.Claims;

namespace MyWarehouse.Services.Security
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationService(
            IRolePermissionRepository rolePermissionRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        //Permessi dinamici dal DB
        public async Task<(bool Result, bool OwnOnly)> HasPermissionAsync(int userId, string action)
        {
            bool hasPermission = false;
            bool ownOnly = false;

            try
            {
                var rolePermissions = await _rolePermissionRepository
                    .GetPermissionsByUserIdAndActionAsync(userId, action);

                hasPermission = rolePermissions.Any();
                ownOnly = rolePermissions.All(p => p.OwnOnly);
            }
            catch
            {
                hasPermission = false;
                ownOnly = false;
            }

            return (hasPermission, ownOnly);
        }

        //Recupera l'Id dell'utente corrente dal token
        public int GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
            {
                throw new UnauthorizedAccessException("Utente non autenticato.");
            }

            return int.Parse(userIdString);
        }
    }
}