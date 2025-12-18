using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Infrastructure.Services.Warehouse;
using Parhelion.Tests.Fixtures;
using Xunit;

namespace Parhelion.Tests.Integration;

/// <summary>
/// Tests de integración para el área de WMS (Warehouse Management System).
/// Verifica flujos completos que involucran múltiples servicios WMS.
/// </summary>
public class WMSIntegrationTests : IClassFixture<ServiceTestFixture>
{
    private readonly ServiceTestFixture _fixture;

    public WMSIntegrationTests(ServiceTestFixture fixture) => _fixture = fixture;

    /// <summary>
    /// Flujo completo: Crear zona → Crear producto → Agregar stock → Reservar → Liberar
    /// </summary>
    [Fact]
    public async Task WarehouseFlow_CreateZone_AddStock_ReserveAndRelease()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var zoneService = new WarehouseZoneService(uow);
        var stockService = new InventoryStockService(uow);

        // Act 1: Create new zone
        var zoneResult = await zoneService.CreateAsync(
            new CreateWarehouseZoneRequest(ids.LocationId, "C3", "Zona C3 - Cold Storage", "ColdChain"));
        Assert.True(zoneResult.Success);
        var newZoneId = zoneResult.Data!.Id;

        // Act 2: Add inventory to new zone
        var stockResult = await stockService.CreateAsync(
            new CreateInventoryStockRequest(newZoneId, ids.ProductId, 200, 0, "LOT-COLD-001", DateTime.UtcNow.AddDays(30), 50.00m));
        Assert.True(stockResult.Success);
        var stockId = stockResult.Data!.Id;
        Assert.Equal(200, stockResult.Data.QuantityAvailable);

        // Act 3: Reserve quantity
        var reserveResult = await stockService.ReserveQuantityAsync(stockId, 75);
        Assert.True(reserveResult.Success);
        Assert.Equal(125, reserveResult.Data!.QuantityAvailable);
        Assert.Equal(75, reserveResult.Data.QuantityReserved);

        // Act 4: Release part of reserved
        var releaseResult = await stockService.ReleaseReservedAsync(stockId, 25);
        Assert.True(releaseResult.Success);
        Assert.Equal(150, releaseResult.Data!.QuantityAvailable);
        Assert.Equal(50, releaseResult.Data.QuantityReserved);
    }

    /// <summary>
    /// Flujo: Verificar stock por zona después de múltiples operaciones
    /// </summary>
    [Fact]
    public async Task InventoryTracking_MultipleStocksPerZone()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var stockService = new InventoryStockService(uow);

        // Create multiple stocks in same zone with different batches
        await stockService.CreateAsync(new CreateInventoryStockRequest(ids.ZoneId, ids.ProductId, 100, 0, "LOT-A", DateTime.UtcNow.AddMonths(3), null));
        await stockService.CreateAsync(new CreateInventoryStockRequest(ids.ZoneId, ids.ProductId, 150, 0, "LOT-B", DateTime.UtcNow.AddMonths(6), null));

        // Act: Get all stocks for zone
        var request = new PagedRequest { Page = 1, PageSize = 20 };
        var zoneStocks = await stockService.GetByZoneAsync(ids.ZoneId, request);

        // Assert: Should have original seeded stock + 2 new ones
        Assert.True(zoneStocks.TotalCount >= 3);
    }

    /// <summary>
    /// Flujo: Verificar que zonas inactivas no afectan queries de zona activa
    /// </summary>
    [Fact]
    public async Task ZoneManagement_ActiveVsInactive()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var zoneService = new WarehouseZoneService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 20 };

        // Create active zone
        await zoneService.CreateAsync(new CreateWarehouseZoneRequest(ids.LocationId, "ACTIVE-1", "Active Zone", "Storage"));
        
        // Create and deactivate zone
        var inactiveResult = await zoneService.CreateAsync(new CreateWarehouseZoneRequest(ids.LocationId, "INACTIVE-1", "Inactive Zone", "Storage"));
        await zoneService.UpdateAsync(inactiveResult.Data!.Id, new UpdateWarehouseZoneRequest("INACTIVE-1", "Inactive Zone", "Storage", false));

        // Act: Get only active zones
        var activeZones = await zoneService.GetActiveAsync(ids.LocationId, request);

        // Assert: Should not include inactive zone
        Assert.True(activeZones.Items.All(z => z.IsActive));
    }

    /// <summary>
    /// Flujo: Fail-safe - Reservar más de lo disponible debe fallar
    /// </summary>
    [Fact]
    public async Task InventoryGuard_CannotOverReserve()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var stockService = new InventoryStockService(uow);

        // Stock has 100 total, 10 reserved = 90 available
        
        // Act: Try to reserve 95 (more than 90 available)
        var result = await stockService.ReserveQuantityAsync(ids.StockId, 95);

        // Assert: Should fail
        Assert.False(result.Success);
        Assert.Contains("insuficiente", result.Message.ToLower());
    }
}
