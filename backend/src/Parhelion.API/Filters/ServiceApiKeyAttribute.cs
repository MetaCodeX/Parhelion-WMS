using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.Interfaces;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Filters;

/// <summary>
/// Filtro de autenticación para servicios externos (n8n, microservicios).
/// 
/// Soporta 2 métodos de autenticación:
/// 1. X-Service-Key: API Key persistente (lookup en BD)
/// 2. Authorization: Bearer {CallbackToken}: JWT de corta duración (validación criptográfica)
/// 
/// El TenantId se almacena en HttpContext.Items["ServiceTenantId"]
/// 
/// Uso: [ServiceApiKey] en métodos o controladores.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ServiceApiKeyAttribute : Attribute, IAsyncActionFilter
{
    private const string ServiceKeyHeader = "X-Service-Key";
    private const string AuthorizationHeader = "Authorization";
    private const string BearerPrefix = "Bearer ";
    public const string TenantIdKey = "ServiceTenantId";
    public const string CorrelationIdKey = "ServiceCorrelationId";

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next)
    {
        // ========== OPCIÓN 1: Callback Token (Bearer JWT) ==========
        if (context.HttpContext.Request.Headers.TryGetValue(AuthorizationHeader, out var authHeader) 
            && authHeader.ToString().StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.ToString()[BearerPrefix.Length..];
            
            var tokenService = context.HttpContext.RequestServices
                .GetService<ICallbackTokenService>();

            if (tokenService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            var claims = tokenService.ValidateCallbackToken(token);
            if (claims != null)
            {
                // Token válido - establecer claims y continuar
                context.HttpContext.Items[TenantIdKey] = claims.TenantId;
                context.HttpContext.Items[CorrelationIdKey] = claims.CorrelationId;
                await next();
                return;
            }

            // Token inválido o expirado
            context.Result = new UnauthorizedObjectResult(new { 
                error = "Invalid or expired callback token" 
            });
            return;
        }

        // ========== OPCIÓN 2: X-Service-Key (API Key persistente) ==========
        if (context.HttpContext.Request.Headers.TryGetValue(ServiceKeyHeader, out var providedKey) 
            && !string.IsNullOrWhiteSpace(providedKey))
        {
            var keyHash = ComputeSha256Hash(providedKey.ToString());

            var dbContext = context.HttpContext.RequestServices
                .GetRequiredService<ParhelionDbContext>();

            var apiKey = await dbContext.ServiceApiKeys
                .AsNoTracking()
                .FirstOrDefaultAsync(k => 
                    k.KeyHash == keyHash && 
                    k.IsActive && 
                    !k.IsDeleted);

            if (apiKey == null)
            {
                context.Result = new UnauthorizedObjectResult(new { 
                    error = "Invalid or inactive service key" 
                });
                return;
            }

            // Validar expiración
            if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow)
            {
                context.Result = new UnauthorizedObjectResult(new { 
                    error = "Service key has expired" 
                });
                return;
            }

            // Almacenar TenantId
            context.HttpContext.Items[TenantIdKey] = apiKey.TenantId;

            // Actualizar LastUsedAt de forma fire-and-forget
            _ = UpdateLastUsedAsync(context, apiKey.Id);

            await next();
            return;
        }

        // ========== SIN CREDENCIALES ==========
        context.Result = new UnauthorizedObjectResult(new { 
            error = $"Missing authentication. Provide {ServiceKeyHeader} header or Authorization: Bearer <token>" 
        });
    }

    private static async Task UpdateLastUsedAsync(ActionExecutingContext context, Guid apiKeyId)
    {
        try
        {
            using var scope = context.HttpContext.RequestServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ParhelionDbContext>();
            var key = await ctx.ServiceApiKeys.FindAsync(apiKeyId);
            if (key != null)
            {
                key.LastUsedAt = DateTime.UtcNow;
                key.LastUsedFromIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();
                await ctx.SaveChangesAsync();
            }
        }
        catch { /* Fire and forget */ }
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }
}
