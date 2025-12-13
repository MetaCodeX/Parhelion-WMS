using Parhelion.Domain.Common;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Ruta predefinida con secuencia de paradas.
/// Usado para asignar envíos a rutas conocidas.
/// </summary>
public class RouteBlueprint : TenantEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int TotalSteps { get; set; }
    public TimeSpan TotalTransitTime { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<RouteStep> Steps { get; set; } = new List<RouteStep>();
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
