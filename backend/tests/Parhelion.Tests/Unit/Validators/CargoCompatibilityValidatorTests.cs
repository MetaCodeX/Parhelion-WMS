using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Validators;
using Xunit;

namespace Parhelion.Tests.Unit.Validators;

/// <summary>
/// Tests para CargoCompatibilityValidator.
/// Verifica las reglas de negocio de compatibilidad carga-camión.
/// </summary>
public class CargoCompatibilityValidatorTests
{
    private readonly CargoCompatibilityValidator _validator = new();

    // ========== REFRIGERATION TESTS ==========

    [Fact]
    public void RefrigeratedCargo_WithRefrigeratedTruck_ReturnsSuccess()
    {
        var items = new[] { CreateItem(requiresRefrigeration: true) };
        
        var result = _validator.ValidateShipmentForTruck(items, TruckType.Refrigerated);
        
        Assert.True(result.IsValid);
    }

    [Fact]
    public void RefrigeratedCargo_WithDryBoxTruck_ReturnsFailure()
    {
        var items = new[] { CreateItem(requiresRefrigeration: true) };
        
        var result = _validator.ValidateShipmentForTruck(items, TruckType.DryBox);
        
        Assert.False(result.IsValid);
        Assert.Equal(TruckType.Refrigerated, result.RequiredTruckType);
        Assert.Contains("cadena de frío", result.ErrorMessage!.ToLower());
    }

    // ========== HAZMAT TESTS ==========

    [Fact]
    public void HazmatCargo_WithHazmatTruck_ReturnsSuccess()
    {
        var items = new[] { CreateItem(isHazardous: true) };
        
        var result = _validator.ValidateShipmentForTruck(items, TruckType.HazmatTank);
        
        Assert.True(result.IsValid);
    }

    [Fact]
    public void HazmatCargo_WithRefrigeratedTruck_ReturnsFailure()
    {
        var items = new[] { CreateItem(isHazardous: true) };
        
        var result = _validator.ValidateShipmentForTruck(items, TruckType.Refrigerated);
        
        Assert.False(result.IsValid);
        Assert.Equal(TruckType.HazmatTank, result.RequiredTruckType);
        Assert.Contains("hazmat", result.ErrorMessage!.ToLower());
    }

    // ========== HIGH VALUE TESTS ==========

    [Fact]
    public void HighValueCargo_WithArmoredTruck_ReturnsSuccess()
    {
        var items = new[] { CreateItem(declaredValue: 600_000m) };
        
        var result = _validator.ValidateShipmentForTruck(items, TruckType.Armored);
        
        Assert.True(result.IsValid);
    }

    [Fact]
    public void HighValueCargo_WithDryBoxTruck_ReturnsFailure()
    {
        var items = new[] { CreateItem(declaredValue: 600_000m) };
        
        var result = _validator.ValidateShipmentForTruck(items, TruckType.DryBox);
        
        Assert.False(result.IsValid);
        Assert.Equal(TruckType.Armored, result.RequiredTruckType);
        Assert.Contains("blindado", result.ErrorMessage!.ToLower());
    }

    // ========== STANDARD CARGO TESTS ==========

    [Fact]
    public void StandardCargo_WithDryBoxTruck_ReturnsSuccess()
    {
        var items = new[] { CreateItem() };
        
        var result = _validator.ValidateShipmentForTruck(items, TruckType.DryBox);
        
        Assert.True(result.IsValid);
    }

    [Fact]
    public void EmptyItems_ReturnsSuccess()
    {
        var items = Array.Empty<ShipmentItem>();
        
        var result = _validator.ValidateShipmentForTruck(items, TruckType.DryBox);
        
        Assert.True(result.IsValid);
    }

    // ========== DETERMINE REQUIRED TYPE TESTS ==========

    [Fact]
    public void DetermineRequiredType_Hazmat_ReturnsHazmatTank()
    {
        var items = new[] { CreateItem(isHazardous: true) };
        
        var result = _validator.DetermineRequiredTruckType(items);
        
        Assert.Equal(TruckType.HazmatTank, result);
    }

    [Fact]
    public void DetermineRequiredType_Refrigerated_ReturnsRefrigerated()
    {
        var items = new[] { CreateItem(requiresRefrigeration: true) };
        
        var result = _validator.DetermineRequiredTruckType(items);
        
        Assert.Equal(TruckType.Refrigerated, result);
    }

    [Fact]
    public void DetermineRequiredType_HighValue_ReturnsArmored()
    {
        var items = new[] { CreateItem(declaredValue: 600_000m) };
        
        var result = _validator.DetermineRequiredTruckType(items);
        
        Assert.Equal(TruckType.Armored, result);
    }

    [Fact]
    public void DetermineRequiredType_Standard_ReturnsDryBox()
    {
        var items = new[] { CreateItem() };
        
        var result = _validator.DetermineRequiredTruckType(items);
        
        Assert.Equal(TruckType.DryBox, result);
    }

    private static ShipmentItem CreateItem(
        bool requiresRefrigeration = false,
        bool isHazardous = false,
        decimal declaredValue = 1000m) => new()
    {
        Id = Guid.NewGuid(),
        ShipmentId = Guid.NewGuid(),
        Description = "Test Item",
        Quantity = 1,
        WeightKg = 10,
        WidthCm = 50,
        HeightCm = 50,
        LengthCm = 50,
        RequiresRefrigeration = requiresRefrigeration,
        IsHazardous = isHazardous,
        DeclaredValue = declaredValue,
        CreatedAt = DateTime.UtcNow
    };
}
