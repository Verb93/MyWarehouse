using Microsoft.AspNetCore.Identity;
using MyWarehouse.Common.Security.SecurityInterface;

namespace MyWarehouse.Common.Security;

public class PasswordService<TUser> : IPasswordService<TUser> where TUser : class
{
    private readonly PasswordHasher<object> _passHasher;

    public PasswordService()
    {
        _passHasher = new PasswordHasher<object>();
    }
    public string HashPassword(TUser user, string password)
    {
        return _passHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(TUser user, string hashedPassword, string providedPassword)
    {
        return _passHasher.VerifyHashedPassword(user, hashedPassword, providedPassword) == PasswordVerificationResult.Success;
    }
}
