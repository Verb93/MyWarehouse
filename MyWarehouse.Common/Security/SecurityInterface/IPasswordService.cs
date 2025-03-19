namespace MyWarehouse.Common.Security.SecurityInterface;

public interface IPasswordService<TUser> where TUser : class
{
    string HashPassword(TUser user, string password);
    bool VerifyPassword(TUser user, string hashedPassword, string providedPassword);
}
