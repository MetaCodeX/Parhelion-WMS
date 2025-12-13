using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Evento de trazabilidad del envío.
/// INMUTABLE: Los checkpoints no se modifican, solo se agregan nuevos.
/// </summary>
public class ShipmentCheckpoint : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public Guid? LocationId { get; set; }
    public CheckpointStatus StatusCode { get; set; }
    public string? Remarks { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid CreatedByUserId { get; set; }
    
    // ========== TRAZABILIDAD DE CARGUEROS ==========
    
    /// <summary>Chofer que manejó el paquete en este checkpoint</summary>
    public Guid? HandledByDriverId { get; set; }
    
    /// <summary>Camión donde se cargó el paquete</summary>
    public Guid? LoadedOntoTruckId { get; set; }
    
    /// <summary>Tipo de acción: Loaded, Unloaded, Transferred, Delivered, etc.</summary>
    public string? ActionType { get; set; }
    
    /// <summary>Nombre del custodio anterior (quien entregó)</summary>
    public string? PreviousCustodian { get; set; }
    
    /// <summary>Nombre del nuevo custodio (quien recibió)</summary>
    public string? NewCustodian { get; set; }

    // Navigation Properties
    public Shipment Shipment { get; set; } = null!;
    public Location? Location { get; set; }
    public User CreatedBy { get; set; } = null!;
    public Driver? HandledByDriver { get; set; }
    public Truck? LoadedOntoTruck { get; set; }
}
