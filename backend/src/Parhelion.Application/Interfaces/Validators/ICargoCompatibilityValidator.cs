using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Application.Interfaces.Validators;

/// <summary>
/// Resultado de validación de compatibilidad carga-camión.
/// </summary>
public record CargoValidationResult(bool IsValid, string? ErrorMessage = null, TruckType? RequiredTruckType = null)
{
    public static CargoValidationResult Success() => new(true);
    public static CargoValidationResult Fail(string message, TruckType? required = null) => new(false, message, required);
}

/// <summary>
/// Validador de compatibilidad entre carga y tipo de camión.
/// Implementa reglas de negocio del README:
/// - Cargo refrigerado → Truck Refrigerated
/// - Cargo HAZMAT → Truck HazmatTank
/// - Cargo alto valor → Truck Armored
/// </summary>
public interface ICargoCompatibilityValidator
{
    /// <summary>
    /// Valida si un envío puede asignarse a un camión específico.
    /// </summary>
    CargoValidationResult ValidateShipmentForTruck(IEnumerable<ShipmentItem> items, TruckType truckType);

    /// <summary>
    /// Determina qué tipo de camión requiere un conjunto de items.
    /// </summary>
    TruckType DetermineRequiredTruckType(IEnumerable<ShipmentItem> items);

    /// <summary>
    /// Threshold de valor declarado para requerir camión blindado.
    /// Configurable por tenant en futuras versiones.
    /// </summary>
    decimal HighValueThreshold { get; }
}
