namespace MyWarehouse.Common.Security;

public class JwtSetting
{
    public string Key { get; set; }
    public int ExpirationInMinutes { get; set; } 
    public string Issuer {  get; set; }
    public string Audience { get; set; }
}
