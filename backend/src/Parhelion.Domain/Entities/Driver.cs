using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Extensión de Employee para choferes.
/// Contiene datos específicos de licencia de conducir y asignación de camiones.
/// Los datos legales (RFC, NSS, CURP, etc.) están en Employee.
/// </summary>
public class Driver : BaseEntity
{
    /// <summary>FK a Employee (datos de empleado)</summary>
    public Guid EmployeeId { get; set; }
    
    // ========== DATOS DE LICENCIA ==========
    
    /// <summary>Número de licencia de conducir</summary>
    public string LicenseNumber { get; set; } = null!;
    
    /// <summary>Tipo de licencia: A, B, C, D, E (Federal)</summary>
    public string? LicenseType { get; set; }
    
    /// <summary>Fecha de vencimiento de la licencia</summary>
    public DateTime? LicenseExpiration { get; set; }
    
    // ========== ASIGNACIÓN DE CAMIONES ==========
    
    /// <summary>
    /// Camión fijo asignado al chofer ("su unidad").
    /// </summary>
    public Guid? DefaultTruckId { get; set; }
    
    /// <summary>
    /// Camión que conduce actualmente (puede diferir del fijo).
    /// Es una caché del último registro de FleetLog.
    /// </summary>
    public Guid? CurrentTruckId { get; set; }
    
    public DriverStatus Status { get; set; }

    // ========== NAVIGATION PROPERTIES ==========
    
    public Employee Employee { get; set; } = null!;
    public Truck? DefaultTruck { get; set; }
    public Truck? CurrentTruck { get; set; }
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    public ICollection<FleetLog> FleetHistory { get; set; } = new List<FleetLog>();
    public ICollection<ShipmentCheckpoint> HandledCheckpoints { get; set; } = new List<ShipmentCheckpoint>();
}
