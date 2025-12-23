using Parhelion.Domain.Common;

namespace Parhelion.Domain.Entities;

/// <summary>
/// API Key para autenticación de servicios externos (n8n, microservicios, integraciones).
/// Cada key pertenece a un Tenant específico, permitiendo multi-tenant en llamadas sin JWT.
/// 
/// Uso: Header X-Service-Key en requests a endpoints protegidos con [ServiceApiKey].
/// </summary>
public class ServiceApiKey : TenantEntity
{
    /// <summary>Hash SHA256 de la API Key (nunca almacenar plain text)</summary>
    public string KeyHash { get; set; } = null!;
    
    /// <summary>Nombre descriptivo para identificar la key (ej: "n8n-production", "webhook-test")</summary>
    public string Name { get; set; } = null!;
    
    /// <summary>Descripción del propósito de esta key</summary>
    public string? Description { get; set; }
    
    /// <summary>Fecha de expiración (null = no expira)</summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>Último uso registrado</summary>
    public DateTime? LastUsedAt { get; set; }
    
    /// <summary>IP desde la que se usó por última vez</summary>
    public string? LastUsedFromIp { get; set; }
    
    /// <summary>Si la key está activa (soft-disable sin eliminar)</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Scopes permitidos (comma-separated). 
    /// Ej: "drivers:read,notifications:write"
    /// Null = acceso completo a endpoints con [ServiceApiKey].
    /// </summary>
    public string? Scopes { get; set; }
    
    // ========== NAVIGATION ==========
    public Tenant Tenant { get; set; } = null!;
}
