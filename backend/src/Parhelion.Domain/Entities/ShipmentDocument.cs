using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Documento B2B asociado a un envío.
/// Tipos: ServiceOrder, Waybill (Carta Porte), Manifest, TripSheet, POD.
/// </summary>
public class ShipmentDocument : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string FileUrl { get; set; } = null!;
    
    /// <summary>
    /// "System" para documentos automáticos, "User" para uploads manuales.
    /// </summary>
    public string GeneratedBy { get; set; } = null!;
    
    public DateTime GeneratedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Navigation Properties
    public Shipment Shipment { get; set; } = null!;
}
