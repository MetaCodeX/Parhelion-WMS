using Parhelion.Application.Interfaces.Validators;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Validators;

/// <summary>
/// Implementación de las reglas de compatibilidad carga-camión.
/// Valida según el README:
/// - Refrigerado → Refrigerated
/// - HAZMAT → HazmatTank  
/// - Alto valor → Armored
/// </summary>
public class CargoCompatibilityValidator : ICargoCompatibilityValidator
{
    /// <summary>
    /// Umbral de valor para requerir camión blindado (MXN).
    /// </summary>
    public decimal HighValueThreshold => 500_000m;

    public CargoValidationResult ValidateShipmentForTruck(IEnumerable<ShipmentItem> items, TruckType truckType)
    {
        var itemList = items.ToList();
        if (!itemList.Any()) return CargoValidationResult.Success();

        // Check refrigeration requirement
        var needsRefrigeration = itemList.Any(i => i.RequiresRefrigeration);
        if (needsRefrigeration && truckType != TruckType.Refrigerated)
        {
            return CargoValidationResult.Fail(
                "Carga requiere cadena de frío. Asigne un camión Refrigerado.",
                TruckType.Refrigerated);
        }

        // Check HAZMAT requirement
        var hasHazmat = itemList.Any(i => i.IsHazardous);
        if (hasHazmat && truckType != TruckType.HazmatTank)
        {
            return CargoValidationResult.Fail(
                "Carga contiene materiales peligrosos (HAZMAT). Asigne un camión HazmatTank.",
                TruckType.HazmatTank);
        }

        // Check high value requirement
        var totalDeclaredValue = itemList.Sum(i => i.DeclaredValue * i.Quantity);
        if (totalDeclaredValue > HighValueThreshold && truckType != TruckType.Armored)
        {
            return CargoValidationResult.Fail(
                $"Valor declarado ({totalDeclaredValue:C}) excede umbral de alto valor ({HighValueThreshold:C}). Asigne un camión Blindado.",
                TruckType.Armored);
        }

        return CargoValidationResult.Success();
    }

    public TruckType DetermineRequiredTruckType(IEnumerable<ShipmentItem> items)
    {
        var itemList = items.ToList();
        if (!itemList.Any()) return TruckType.DryBox;

        // Priority order: HAZMAT > Refrigerated > Armored > DryBox
        if (itemList.Any(i => i.IsHazardous))
            return TruckType.HazmatTank;

        if (itemList.Any(i => i.RequiresRefrigeration))
            return TruckType.Refrigerated;

        var totalValue = itemList.Sum(i => i.DeclaredValue * i.Quantity);
        if (totalValue > HighValueThreshold)
            return TruckType.Armored;

        return TruckType.DryBox;
    }
}
