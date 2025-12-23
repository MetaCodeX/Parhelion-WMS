namespace Parhelion.Application.Interfaces;

/// <summary>
/// Servicio para generar y validar tokens de callback para webhooks.
/// Los tokens son JWT firmados de corta duración que permiten a n8n
/// autenticarse de vuelta al API sin almacenar API keys.
/// </summary>
public interface ICallbackTokenService
{
    /// <summary>
    /// Genera un token de callback para un tenant específico.
    /// </summary>
    /// <param name="tenantId">ID del tenant para el que se genera el token.</param>
    /// <param name="correlationId">ID de correlación para trazabilidad.</param>
    /// <returns>JWT firmado con claims de tenant y expiración.</returns>
    string GenerateCallbackToken(Guid tenantId, Guid correlationId);
    
    /// <summary>
    /// Valida un token de callback y extrae los claims.
    /// </summary>
    /// <param name="token">Token JWT a validar.</param>
    /// <returns>Claims si válido, null si inválido o expirado.</returns>
    CallbackTokenClaims? ValidateCallbackToken(string token);
}

/// <summary>
/// Claims extraídos de un token de callback válido.
/// </summary>
public record CallbackTokenClaims(
    Guid TenantId,
    Guid CorrelationId,
    DateTime ExpiresAt
);
