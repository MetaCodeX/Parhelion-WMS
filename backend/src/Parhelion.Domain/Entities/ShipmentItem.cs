using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Partida individual dentro de un envío (SKU, dimensiones, peso).
/// Incluye cálculo de peso volumétrico para cotizaciones.
/// </summary>
public class ShipmentItem : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public string? Sku { get; set; }
    public string Description { get; set; } = null!;
    public PackagingType PackagingType { get; set; }
    public int Quantity { get; set; }
    public decimal WeightKg { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public decimal LengthCm { get; set; }
    
    /// <summary>
    /// Volumen calculado en metros cúbicos.
    /// </summary>
    public decimal VolumeM3 => (WidthCm * HeightCm * LengthCm) / 1_000_000m;
    
    /// <summary>
    /// Peso volumétrico para cotización.
    /// Fórmula: (Largo × Ancho × Alto) / 5000
    /// </summary>
    public decimal VolumetricWeightKg => (WidthCm * HeightCm * LengthCm) / 5000m;
    
    /// <summary>
    /// Valor monetario para seguro.
    /// </summary>
    public decimal DeclaredValue { get; set; }
    
    public bool IsFragile { get; set; }
    public bool IsHazardous { get; set; }
    public bool RequiresRefrigeration { get; set; }
    public string? StackingInstructions { get; set; }

    // Navigation Properties
    public Shipment Shipment { get; set; } = null!;
}
