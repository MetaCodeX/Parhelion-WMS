using Parhelion.Domain.Common;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Representa a cada cliente/empresa que usa el sistema.
/// Root de multi-tenancy - aísla todos los datos por cliente.
/// </summary>
public class Tenant : BaseEntity
{
    public string CompanyName { get; set; } = null!;
    public string ContactEmail { get; set; } = null!;
    public int FleetSize { get; set; }
    public int DriverCount { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Truck> Trucks { get; set; } = new List<Truck>();
    public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
    public ICollection<Location> Locations { get; set; } = new List<Location>();
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    public ICollection<RouteBlueprint> RouteBlueprints { get; set; } = new List<RouteBlueprint>();
    public ICollection<NetworkLink> NetworkLinks { get; set; } = new List<NetworkLink>();
    public ICollection<FleetLog> FleetLogs { get; set; } = new List<FleetLog>();
}
