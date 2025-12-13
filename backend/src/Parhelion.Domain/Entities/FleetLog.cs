using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Bitácora de cambios de vehículo.
/// SOURCE OF TRUTH para la asignación Chofer-Camión.
/// Driver.CurrentTruckId es solo una caché del último registro.
/// </summary>
public class FleetLog : TenantEntity
{
    public Guid DriverId { get; set; }
    
    /// <summary>
    /// Nullable si el chofer no tenía camión asignado antes.
    /// </summary>
    public Guid? OldTruckId { get; set; }
    
    public Guid NewTruckId { get; set; }
    public FleetLogReason Reason { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid CreatedByUserId { get; set; }

    // Navigation Properties
    public Tenant Tenant { get; set; } = null!;
    public Driver Driver { get; set; } = null!;
    public Truck? OldTruck { get; set; }
    public Truck NewTruck { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
}
