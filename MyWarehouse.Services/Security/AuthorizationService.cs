using MyWarehouse.Interfaces.SecurityInterface;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using MyWarehouse.Data;
using MyWarehouse.Data.Models;
using System.Text.RegularExpressions;

namespace MyWarehouse.Services.Security;

public class AuthorizationService : IAuthorizationService
{
    private readonly WarehouseContext _context;
    private readonly ISupplierUserRepository _supplierUserRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationService(
        WarehouseContext context,
        ISupplierUserRepository supplierUserRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _supplierUserRepository = supplierUserRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    //metodo per controllare se l'utente ha accesso a una risorsa di sua proprietà
    //controlla il claim "Permission" nel token con ownOnly = true
    //recupera l'entità dal database e confronta l'id del proprietario
    //restituisce true solo se la risorsa è effettivamente dell'utente
    public async Task<bool> CanAccessOwnResourceAsync<T>(
        ClaimsPrincipal user,
        int resourceId,
        Func<T, int> getOwnerIdFunc
    ) where T : class
    {
        bool hasAccess = false;

        try
        {
            //recupero tutti i permessi dal token
            var permissions = user.FindAll("Permission");
            var method = _httpContextAccessor.HttpContext?.Request.Method.ToUpper();
            var path = _httpContextAccessor.HttpContext?.Request.Path.ToString().ToLower();

            var ownOnlyPermission = permissions.FirstOrDefault(p =>
            {
                var parts = p.Value.Split(':');
                if (parts.Length < 3) return false;

                var permMethod = parts[0];
                var permPath = parts[1];
                var ownOnly = parts[2] == "1";

                return permMethod == method && PathMatches(path, permPath) && ownOnly;
            });

            if (ownOnlyPermission != null)
            {
                var entity = await _context.Set<T>().FindAsync(resourceId);

                if (entity != null)
                {
                    var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                    var supplierIds = await _supplierUserRepository.GetSupplierIdsByUserId(userId);
                    var ownerId = getOwnerIdFunc(entity);
                    hasAccess = supplierIds.Contains(ownerId);
                }
            }
        }
        catch
        {
            hasAccess = false;
        }

        return hasAccess;
    }

    //overload del metodo CanAccessOwnResourceAsync
    //recupera automaticamente l'utente dal contesto HTTP
    //utile per non dover passare ClaimsPrincipal nei service
    public async Task<bool> CanAccessOwnResourceAsync<T>(
        int resourceId,
        Func<T, int> getOwnerIdFunc
    ) where T : class
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return false;

        return await CanAccessOwnResourceAsync(user, resourceId, getOwnerIdFunc);
    }

    // metodo per verificare se un utente può annullare un ordine
    // gli admin possono sempre annullare qualsiasi ordine
    // i client e gli usersupplier possono annullare solo i propri ordini (order.IdUser == userId)
    // i supplier puri non possono annullare ordini
    public Task<bool> CanCancelOrderAsync(Orders order, int userId, string role)
    {
        bool hasAccess = false;

        if (role == "admin")
        {
            hasAccess = true;
        }
        else if (role == "client" || role == "usersupplier")
        {
            hasAccess = order.IdUser == userId;
        }

        return Task.FromResult(hasAccess);
    }

    public async Task<bool> CanUpdateOrderStatusAsync(Orders order, int userId, string role)
    {
        bool hasAccess = false;

        if (role == "admin")
        {
            hasAccess = true;
        }
        else if (role == "usersupplier")
        {
            var supplierIds = await _supplierUserRepository.GetSupplierIdsByUserId(userId);
            var orderSupplierIds = order.OrderDetails
                .Where(od => od.Product != null && od.Product.IdSupplier.HasValue)
                .Select(od => od.Product!.IdSupplier!.Value)
                .Distinct();

            hasAccess = supplierIds.Intersect(orderSupplierIds).Any();
        }

        return hasAccess;
    }

    public Task<bool> CanAccessOrdersByUserAsync(int requestedUserId, int callerUserId, string role)
    {
        bool hasAccess = false;

        if (role == "admin")
        {
            hasAccess = true;
        }
        else if (role == "client" || role == "usersupplier")
        {
            hasAccess = requestedUserId == callerUserId;
        }

        return Task.FromResult(hasAccess);
    }

    public async Task<bool> CanAccessOrderWithDetailsAsync(Orders order, int userId, string role)
    {
        bool hasAccess = false;

        if (role == "admin")
        {
            hasAccess = true;
        }
        else if (role == "client" || role == "usersupplier")
        {
            // accesso da client (proprietario ordine)
            if (order.IdUser == userId)
            {
                hasAccess = true;
            }
            else if (role == "usersupplier")
            {
                // accesso da supplier (ordine con propri prodotti)
                var supplierIds = await _supplierUserRepository.GetSupplierIdsByUserId(userId);
                var orderSupplierIds = order.OrderDetails
                    .Where(od => od.Product?.IdSupplier != null)
                    .Select(od => od.Product!.IdSupplier!.Value)
                    .Distinct();

                hasAccess = supplierIds.Intersect(orderSupplierIds).Any();
            }
        }

        return hasAccess;
    }



    //funzione che confronta il path della richiesta con il pattern del permesso
    //es: /api/products/33 ↔ /api/products/{id}
    private bool PathMatches(string actualPath, string templatePath)
    {
        var pattern = "^" + Regex.Replace(templatePath, @"\{[^/]+\}", @"[^/]+") + "$";
        return Regex.IsMatch(actualPath, pattern, RegexOptions.IgnoreCase);
    }
}