using Parhelion.Domain.Common;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Usuario del sistema (Admin, Chofer, Almacenista, Demo).
/// El password se hashea con BCrypt para usuarios normales
/// y Argon2id para credenciales administrativas.
/// </summary>
public class User : TenantEntity
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public Guid RoleId { get; set; }
    
    /// <summary>
    /// True si el usuario fue creado para sesión de demo temporal.
    /// </summary>
    public bool IsDemoUser { get; set; }
    
    /// <summary>
    /// True si las credenciales usan Argon2id (admin security).
    /// False si usan BCrypt (estándar).
    /// </summary>
    public bool UsesArgon2 { get; set; }
    
    public DateTime? LastLogin { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public Tenant Tenant { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public Driver? Driver { get; set; }
    public ICollection<ShipmentCheckpoint> CreatedCheckpoints { get; set; } = new List<ShipmentCheckpoint>();
    public ICollection<FleetLog> CreatedFleetLogs { get; set; } = new List<FleetLog>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
