using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Envío principal con origen, destino, ruta asignada y trazabilidad.
/// Genera tracking number único con formato PAR-XXXXXX.
/// </summary>
public class Shipment : TenantEntity
{
    public string TrackingNumber { get; set; } = null!;
    public string QrCodeData { get; set; } = null!;
    public Guid OriginLocationId { get; set; }
    public Guid DestinationLocationId { get; set; }

    // ========== CLIENTE REMITENTE/DESTINATARIO ==========
    
    /// <summary>Cliente que envía el paquete (opcional, puede ser registro manual)</summary>
    public Guid? SenderId { get; set; }
    
    /// <summary>Cliente que recibe el paquete (opcional, puede ser registro manual)</summary>
    public Guid? RecipientClientId { get; set; }

    // Enrutamiento Hub & Spoke
    public Guid? AssignedRouteId { get; set; }
    public int? CurrentStepOrder { get; set; }

    // Datos de destinatario (para envíos sin cliente registrado)
    public string RecipientName { get; set; } = null!;
    public string? RecipientPhone { get; set; }
    public decimal TotalWeightKg { get; set; }
    public decimal TotalVolumeM3 { get; set; }
    public decimal? DeclaredValue { get; set; }

    // Campos B2B (Documentación Legal)
    public string? SatMerchandiseCode { get; set; }
    public string? DeliveryInstructions { get; set; }
    public string? RecipientSignatureUrl { get; set; }

    public ShipmentPriority Priority { get; set; }
    public ShipmentStatus Status { get; set; }
    public Guid? TruckId { get; set; }
    public Guid? DriverId { get; set; }
    
    /// <summary>
    /// True si la carga se realizó por escaneo QR.
    /// </summary>
    public bool WasQrScanned { get; set; }
    
    /// <summary>
    /// True si hay retraso (avería, tráfico).
    /// </summary>
    public bool IsDelayed { get; set; }

    // Fechas y Ventanas
    public DateTime? ScheduledDeparture { get; set; }
    public DateTime? PickupWindowStart { get; set; }
    public DateTime? PickupWindowEnd { get; set; }
    public DateTime? EstimatedArrival { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    // Navigation Properties
    public Tenant Tenant { get; set; } = null!;
    public Location OriginLocation { get; set; } = null!;
    public Location DestinationLocation { get; set; } = null!;
    public RouteBlueprint? AssignedRoute { get; set; }
    public Truck? Truck { get; set; }
    public Driver? Driver { get; set; }
    
    /// <summary>Cliente remitente (quien envía)</summary>
    public Client? Sender { get; set; }
    
    /// <summary>Cliente destinatario (quien recibe)</summary>
    public Client? RecipientClient { get; set; }
    
    public ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
    public ICollection<ShipmentCheckpoint> History { get; set; } = new List<ShipmentCheckpoint>();
    public ICollection<ShipmentDocument> Documents { get; set; } = new List<ShipmentDocument>();
}
