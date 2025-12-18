using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Infrastructure.Services.Warehouse;
using Parhelion.Tests.Fixtures;
using Xunit;

namespace Parhelion.Tests.Unit.Services.Warehouse;

/// <summary>
/// Tests para WarehouseZoneService.
/// </summary>
public class WarehouseZoneServiceTests : IClassFixture<ServiceTestFixture>
{
    private readonly ServiceTestFixture _fixture;

    public WarehouseZoneServiceTests(ServiceTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new WarehouseZoneService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.GetAllAsync(request);

        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingZone_ReturnsZone()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new WarehouseZoneService(uow);

        var result = await service.GetByIdAsync(ids.ZoneId);

        Assert.NotNull(result);
        Assert.Equal("A1", result.Code);
    }

    [Fact]
    public async Task GetByLocationAsync_ReturnsZonesForLocation()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new WarehouseZoneService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.GetByLocationAsync(ids.LocationId, request);

        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
    }

    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsZone()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new WarehouseZoneService(uow);

        var result = await service.GetByCodeAsync(ids.LocationId, "A1");

        Assert.NotNull(result);
        Assert.Equal(ids.ZoneId, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new WarehouseZoneService(uow);
        var request = new CreateWarehouseZoneRequest(ids.LocationId, "B2", "Zona B2 - Storage", "Storage");

        var result = await service.CreateAsync(request);

        Assert.True(result.Success);
        Assert.Equal("B2", result.Data!.Code);
    }

    [Fact]
    public async Task UpdateAsync_ExistingZone_ReturnsSuccess()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new WarehouseZoneService(uow);
        var request = new UpdateWarehouseZoneRequest("A1-UPD", "Zona A1 Updated", "Storage", true);

        var result = await service.UpdateAsync(ids.ZoneId, request);

        Assert.True(result.Success);
        Assert.Equal("A1-UPD", result.Data!.Code);
    }

    [Fact]
    public async Task ExistsAsync_ExistingZone_ReturnsTrue()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new WarehouseZoneService(uow);

        var exists = await service.ExistsAsync(ids.ZoneId);

        Assert.True(exists);
    }
}
