using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Parhelion.Application.Auth;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Auth;

/// <summary>
/// Implementación del servicio JWT para generación y validación de tokens.
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly int _accessTokenExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        
        var secretKey = _configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("JWT SecretKey not configured");
            
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        _accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "120");
        _refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
    }

    /// <inheritdoc />
    public string GenerateAccessToken(User user, string roleName)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, roleName),
            new("tenant_id", user.TenantId.ToString()),
            new("is_demo", user.IsDemoUser.ToString().ToLower())
        };

        // Agregar permisos del rol como claims
        var permissions = RolePermissions.GetPermissions(roleName);
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission.ToString()));
        }

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var expiration = GetAccessTokenExpiration();

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "Parhelion",
            audience: _configuration["Jwt:Audience"] ?? "ParhelionClient",
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <inheritdoc />
    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "Parhelion",
                ValidAudience = _configuration["Jwt:Audience"] ?? "ParhelionClient",
                IssuerSigningKey = _signingKey,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public DateTime GetAccessTokenExpiration() 
        => DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes);

    /// <inheritdoc />
    public DateTime GetRefreshTokenExpiration() 
        => DateTime.UtcNow.AddDays(_refreshTokenExpirationDays);
}
