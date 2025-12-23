using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Parhelion.Application.Interfaces;

namespace Parhelion.Infrastructure.Services.Auth;

/// <summary>
/// Implementación de ICallbackTokenService usando JWT firmados.
/// Los tokens son de corta duración (15 minutos) y contienen TenantId + CorrelationId.
/// </summary>
public class CallbackTokenService : ICallbackTokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly TimeSpan _tokenLifetime = TimeSpan.FromMinutes(15);

    public CallbackTokenService(IConfiguration configuration)
    {
        _secretKey = Environment.GetEnvironmentVariable("JWT_SECRET") 
            ?? configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("JWT_SECRET is required");
        
        _issuer = configuration["Jwt:Issuer"] ?? "Parhelion";
    }

    /// <inheritdoc />
    public string GenerateCallbackToken(Guid tenantId, Guid correlationId)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("tenant_id", tenantId.ToString()),
            new Claim("correlation_id", correlationId.ToString()),
            new Claim("token_type", "callback"), // Distinguir de tokens de usuario
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: "n8n-callback", // Audience específico para callbacks
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.Add(_tokenLifetime),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public CallbackTokenClaims? ValidateCallbackToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = "n8n-callback",
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ClockSkew = TimeSpan.FromSeconds(30) // Tolerancia mínima
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // Verificar que es un callback token
            var tokenTypeClaim = principal.FindFirst("token_type");
            if (tokenTypeClaim?.Value != "callback")
            {
                return null;
            }

            var tenantIdClaim = principal.FindFirst("tenant_id");
            var correlationIdClaim = principal.FindFirst("correlation_id");

            if (tenantIdClaim == null || correlationIdClaim == null)
            {
                return null;
            }

            if (!Guid.TryParse(tenantIdClaim.Value, out var tenantId) ||
                !Guid.TryParse(correlationIdClaim.Value, out var correlationId))
            {
                return null;
            }

            return new CallbackTokenClaims(
                tenantId,
                correlationId,
                validatedToken.ValidTo
            );
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}
