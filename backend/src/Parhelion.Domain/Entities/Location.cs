using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Nodo de la red logística: Almacenes, Hubs, Cross-docks, puntos de venta.
/// Código único estilo aeropuerto (MTY, GDL, MM).
/// </summary>
public class Location : TenantEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public LocationType Type { get; set; }
    public string FullAddress { get; set; } = null!;
    
    /// <summary>
    /// Flag: Puede recibir mercancía.
    /// </summary>
    public bool CanReceive { get; set; }
    
    /// <summary>
    /// Flag: Puede despachar mercancía.
    /// </summary>
    public bool CanDispatch { get; set; }
    
    /// <summary>
    /// True si es ubicación propia, false si es externa (cliente/proveedor).
    /// </summary>
    public bool IsInternal { get; set; }
    
    public bool IsActive { get; set; }

    // Navigation Properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Shipment> OriginShipments { get; set; } = new List<Shipment>();
    public ICollection<Shipment> DestinationShipments { get; set; } = new List<Shipment>();
    public ICollection<ShipmentCheckpoint> Checkpoints { get; set; } = new List<ShipmentCheckpoint>();
    public ICollection<RouteStep> RouteSteps { get; set; } = new List<RouteStep>();
    public ICollection<NetworkLink> OutgoingLinks { get; set; } = new List<NetworkLink>();
    public ICollection<NetworkLink> IncomingLinks { get; set; } = new List<NetworkLink>();
}
