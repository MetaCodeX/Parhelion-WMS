using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Enlace de red logística (Lista de Adyacencia).
/// Define conexiones permitidas entre ubicaciones.
/// </summary>
public class NetworkLink : TenantEntity
{
    public Guid OriginLocationId { get; set; }
    public Guid DestinationLocationId { get; set; }
    public NetworkLinkType LinkType { get; set; }
    public TimeSpan TransitTime { get; set; }
    public bool IsBidirectional { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public Tenant Tenant { get; set; } = null!;
    public Location OriginLocation { get; set; } = null!;
    public Location DestinationLocation { get; set; } = null!;
}
