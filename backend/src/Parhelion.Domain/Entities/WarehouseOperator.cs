using Parhelion.Domain.Common;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Extensión de Employee para almacenistas.
/// Similar a Driver pero para operadores de bodega.
/// </summary>
public class WarehouseOperator : BaseEntity
{
    public Guid EmployeeId { get; set; }
    
    /// <summary>Ubicación (bodega) donde trabaja</summary>
    public Guid AssignedLocationId { get; set; }
    
    /// <summary>Zona principal de responsabilidad (nullable)</summary>
    public Guid? PrimaryZoneId { get; set; }
    
    // ========== NAVIGATION PROPERTIES ==========
    
    public Employee Employee { get; set; } = null!;
    public Location AssignedLocation { get; set; } = null!;
    public WarehouseZone? PrimaryZone { get; set; }
    public ICollection<ShipmentCheckpoint> HandledCheckpoints { get; set; } = new List<ShipmentCheckpoint>();
}
