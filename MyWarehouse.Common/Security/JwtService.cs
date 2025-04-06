﻿using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Common.Security.SecurityInterface;
using MyWarehouse.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyWarehouse.Common.Security;

public class JwtService : IJwtService
{
    private readonly JwtSetting _jwtSetting;

    public JwtService(IOptions<JwtSetting> jwtOptions)
    {
        _jwtSetting = jwtOptions.Value;
    }

    public string GenerateJwtToken(UserDTO userDTO, List<RolePermissions> permissions)
    {
        var key = Encoding.ASCII.GetBytes(_jwtSetting.Key);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userDTO.Id.ToString()),
            new Claim(ClaimTypes.Name, userDTO.Email),
            new Claim(ClaimTypes.Role, userDTO.RoleName ?? "User")
        };

        var permissionClaims = permissions.Select(p =>
        new Claim("Permission", $"{p.Permission.HttpVerb}:{p.Permission.Endpoint}:{(p.OwnOnly ? "1" : "0")}")
        );
        claims.AddRange(permissionClaims);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSetting.ExpirationInMinutes),
            Issuer = _jwtSetting.Issuer,
            Audience = _jwtSetting.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
