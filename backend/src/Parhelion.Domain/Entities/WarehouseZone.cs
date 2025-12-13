using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Zona dentro de una ubicación (bodega/almacén).
/// Permite dividir las ubicaciones en áreas funcionales.
/// </summary>
public class WarehouseZone : BaseEntity
{
    public Guid LocationId { get; set; }
    
    /// <summary>Código corto de la zona (A1, B2, COLD-1)</summary>
    public string Code { get; set; } = null!;
    
    /// <summary>Nombre descriptivo de la zona</summary>
    public string Name { get; set; } = null!;
    
    /// <summary>Tipo funcional de la zona</summary>
    public WarehouseZoneType Type { get; set; }
    
    /// <summary>Si la zona está activa</summary>
    public bool IsActive { get; set; } = true;
    
    // ========== NAVIGATION PROPERTIES ==========
    
    public Location Location { get; set; } = null!;
    public ICollection<WarehouseOperator> AssignedOperators { get; set; } = new List<WarehouseOperator>();
}
