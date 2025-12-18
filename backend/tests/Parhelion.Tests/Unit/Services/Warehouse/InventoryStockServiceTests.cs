using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Infrastructure.Services.Warehouse;
using Parhelion.Tests.Fixtures;
using Xunit;

namespace Parhelion.Tests.Unit.Services.Warehouse;

/// <summary>
/// Tests para InventoryStockService.
/// </summary>
public class InventoryStockServiceTests : IClassFixture<ServiceTestFixture>
{
    private readonly ServiceTestFixture _fixture;

    public InventoryStockServiceTests(ServiceTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new InventoryStockService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.GetAllAsync(request);

        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingStock_ReturnsStock()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new InventoryStockService(uow);

        var result = await service.GetByIdAsync(ids.StockId);

        Assert.NotNull(result);
        Assert.Equal(100, result.Quantity);
        Assert.Equal(10, result.QuantityReserved);
        Assert.Equal(90, result.QuantityAvailable);
    }

    [Fact]
    public async Task GetByZoneAsync_ReturnsStockForZone()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new InventoryStockService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.GetByZoneAsync(ids.ZoneId, request);

        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
    }

    [Fact]
    public async Task GetByProductAsync_ReturnsStockForProduct()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new InventoryStockService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.GetByProductAsync(ids.ProductId, request);

        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
    }

    [Fact]
    public async Task ReserveQuantityAsync_SufficientStock_ReturnsSuccess()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new InventoryStockService(uow);

        var result = await service.ReserveQuantityAsync(ids.StockId, 20);

        Assert.True(result.Success);
        Assert.Equal(30, result.Data!.QuantityReserved); // 10 original + 20
        Assert.Equal(70, result.Data!.QuantityAvailable);
    }

    [Fact]
    public async Task ReserveQuantityAsync_InsufficientStock_ReturnsFailure()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new InventoryStockService(uow);

        var result = await service.ReserveQuantityAsync(ids.StockId, 100); // Only 90 available

        Assert.False(result.Success);
        Assert.Contains("insuficiente", result.Message);
    }

    [Fact]
    public async Task ReleaseReservedAsync_ValidQuantity_ReturnsSuccess()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new InventoryStockService(uow);

        var result = await service.ReleaseReservedAsync(ids.StockId, 5);

        Assert.True(result.Success);
        Assert.Equal(5, result.Data!.QuantityReserved); // 10 original - 5
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new InventoryStockService(uow);
        var request = new CreateInventoryStockRequest(ids.ZoneId, ids.ProductId, 50, 0, "LOT-002", DateTime.UtcNow.AddMonths(12), 25.00m);

        var result = await service.CreateAsync(request);

        Assert.True(result.Success);
        Assert.Equal(50, result.Data!.Quantity);
        Assert.Equal("LOT-002", result.Data!.BatchNumber);
    }
}
