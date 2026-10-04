using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using IdentityService.Application.AuthenticationModule.Dto;
using IdentityService.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Application.AuthenticationModule.Service;

/// <summary>
/// Represents the JwtService component.
/// </summary>
public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(
      TokenUserDto user,
      DateTime expiresAt)
    {
        string privateKeyPem =
            _configuration["JwtConfig:PrivateKey"]
            ?? throw new InvalidOperationException(
                "JwtConfig:PrivateKey is not configured.");

        using RSA rsa = RSA.Create();

        rsa.ImportFromPem(
            NormalizePem(privateKeyPem));

        RsaSecurityKey securityKey = new(rsa)
        {
            CryptoProviderFactory = new CryptoProviderFactory
            {
                CacheSignatureProviders = false
            }
        };

        List<Claim> claimList =
        [
            new Claim(
            JwtRegisteredClaimNames.Sub,
            user.UserId.ToString()),

        new Claim(
            ClaimTypes.Name,
            user.UserName),

        new Claim(
            ClaimTypes.Role,
            user.RoleName),

        new Claim(
            "role_id",
            user.RoleId.ToString()),

        new Claim(
            JwtRegisteredClaimNames.Jti,
            Guid.NewGuid().ToString())
        ];

        IEnumerable<Claim> permissionClaims =
           user.PermissionList
               .Where(permission =>
                   !string.IsNullOrWhiteSpace(permission))
               .Select(permission => permission.Trim())
               .Distinct(StringComparer.OrdinalIgnoreCase)
               .Select(permission =>
                   new Claim("permission", permission));

        claimList.AddRange(permissionClaims);

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claimList),
            Issuer = _configuration["JwtConfig:Issuer"],
            Audience = _configuration["JwtConfig:Audience"],
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.RsaSha256)
        };

        JwtSecurityTokenHandler tokenHandler = new();

        SecurityToken securityToken =
            tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(securityToken);
    }
    public ClaimsPrincipal ValidateAccessToken(
     string accessToken)
    {
        string publicKeyPem =
            _configuration["JwtConfig:PublicKey"]
            ?? throw new InvalidOperationException(
                "JwtConfig:PublicKey is not configured.");

        using RSA rsa = RSA.Create();

        rsa.ImportFromPem(
            NormalizePem(publicKeyPem));

        RsaSecurityKey securityKey = new(rsa)
        {
            CryptoProviderFactory = new CryptoProviderFactory
            {
                CacheSignatureProviders = false
            }
        };

        TokenValidationParameters validationParameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,

            ValidateIssuer = true,
            ValidIssuer =
                _configuration["JwtConfig:Issuer"],

            ValidateAudience = true,
            ValidAudience =
                _configuration["JwtConfig:Audience"],

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        JwtSecurityTokenHandler tokenHandler = new();

        return tokenHandler.ValidateToken(
            accessToken,
            validationParameters,
            out _);
    }

    private static string NormalizePem(string pem)
    {
        return pem
            .Replace("\\n", "\n")
            .Trim();
    }
}
