using Microsoft.AspNetCore.Authorization;

namespace MyWarehouse.Common.Security;

public static class Policies
{
    public const string Admin = "admin";
    public const string Client = "client";
    public const string Supplier = "usersupplier";
    public const string AdminOrSupplier = "adminorsupplier";
    public const string ClientOrSupplier = "clientorsupplier";

    public static AuthorizationPolicy AdminPolicy()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(Admin).Build();
    }

    public static AuthorizationPolicy ClientPolicy()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(Client).Build();
    }

    public static AuthorizationPolicy SupplierPolicy()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(Supplier).Build();
    }
    public static AuthorizationPolicy AdminOrSupplierPolicy()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(Admin, Supplier).Build();
    }

    public static AuthorizationPolicy ClientOrSupplierPolicy() 
    { 
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(Client, Supplier).Build(); 
    }

}
