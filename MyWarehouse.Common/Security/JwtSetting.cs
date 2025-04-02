namespace MyWarehouse.Common.Security;

public class JwtSetting
{
    public string Key { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; }
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
