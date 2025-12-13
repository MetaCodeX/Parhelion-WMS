using Parhelion.Domain.Common;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Refresh token para renovar tokens JWT sin re-autenticación.
/// Los tokens tienen expiración de 7 días y pueden ser revocados.
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>Usuario al que pertenece este token</summary>
    public Guid UserId { get; set; }
    
    /// <summary>Token hasheado (nunca almacenar en texto plano)</summary>
    public string TokenHash { get; set; } = null!;
    
    /// <summary>Fecha de expiración del token</summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>Si el token ha sido revocado</summary>
    public bool IsRevoked { get; set; }
    
    /// <summary>Fecha de revocación (si aplica)</summary>
    public DateTime? RevokedAt { get; set; }
    
    /// <summary>Razón de revocación</summary>
    public string? RevokedReason { get; set; }
    
    /// <summary>Dirección IP desde donde se creó</summary>
    public string? CreatedFromIp { get; set; }
    
    /// <summary>User Agent del dispositivo que creó el token</summary>
    public string? UserAgent { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    
    /// <summary>Verifica si el token está activo (no expirado y no revocado)</summary>
    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
}
