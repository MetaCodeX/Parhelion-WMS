using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Chofer de la flotilla con asignación híbrida de camiones.
/// - DefaultTruckId: Camión fijo asignado ("su unidad")
/// - CurrentTruckId: Camión que conduce actualmente (puede diferir)
/// </summary>
public class Driver : TenantEntity
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    
    // ========== DATOS LEGALES ==========
    
    /// <summary>RFC del chofer para nómina</summary>
    public string? Rfc { get; set; }
    
    /// <summary>Número de Seguro Social (IMSS)</summary>
    public string? Nss { get; set; }
    
    /// <summary>CURP del chofer</summary>
    public string? Curp { get; set; }
    
    /// <summary>Número de licencia de conducir</summary>
    public string LicenseNumber { get; set; } = null!;
    
    /// <summary>Tipo de licencia: A, B, C, D, E (Federal)</summary>
    public string? LicenseType { get; set; }
    
    /// <summary>Fecha de vencimiento de la licencia</summary>
    public DateTime? LicenseExpiration { get; set; }
    
    // ========== CONTACTO DE EMERGENCIA ==========
    
    /// <summary>Nombre del contacto de emergencia</summary>
    public string? EmergencyContact { get; set; }
    
    /// <summary>Teléfono del contacto de emergencia</summary>
    public string? EmergencyPhone { get; set; }
    
    // ========== INFORMACIÓN LABORAL ==========
    
    /// <summary>Fecha de contratación</summary>
    public DateTime? HireDate { get; set; }
    
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

    // Navigation Properties
    public Tenant Tenant { get; set; } = null!;
    public User User { get; set; } = null!;
    public Truck? DefaultTruck { get; set; }
    public Truck? CurrentTruck { get; set; }
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    public ICollection<FleetLog> FleetHistory { get; set; } = new List<FleetLog>();
    public ICollection<ShipmentCheckpoint> HandledCheckpoints { get; set; } = new List<ShipmentCheckpoint>();
}
