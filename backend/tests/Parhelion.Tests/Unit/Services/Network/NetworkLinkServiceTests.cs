using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Network;
using Parhelion.Infrastructure.Services.Network;
using Parhelion.Tests.Fixtures;
using Xunit;

namespace Parhelion.Tests.Unit.Services.Network;

/// <summary>
/// Tests para NetworkLinkService.
/// </summary>
public class NetworkLinkServiceTests : IClassFixture<ServiceTestFixture>
{
    private readonly ServiceTestFixture _fixture;

    public NetworkLinkServiceTests(ServiceTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new NetworkLinkService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.GetAllAsync(request);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new NetworkLinkService(uow);
        var request = new CreateNetworkLinkRequest(
            ids.LocationId, ids.Location2Id, "LineHaul", TimeSpan.FromHours(8), true);

        var result = await service.CreateAsync(request);

        Assert.True(result.Success);
        Assert.Equal("Monterrey Hub", result.Data!.OriginLocationName);
        Assert.Equal("Guadalajara Hub", result.Data!.DestinationLocationName);
    }

    [Fact]
    public async Task CreateAsync_InvalidOrigin_ReturnsFailure()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new NetworkLinkService(uow);
        var request = new CreateNetworkLinkRequest(
            Guid.NewGuid(), ids.Location2Id, "LineHaul", TimeSpan.FromHours(8), true);

        var result = await service.CreateAsync(request);

        Assert.False(result.Success);
        Assert.Contains("origen", result.Message.ToLower());
    }

    [Fact]
    public async Task GetByOriginAsync_ReturnsLinksFromLocation()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new NetworkLinkService(uow);
        
        // First create a link
        await service.CreateAsync(new CreateNetworkLinkRequest(
            ids.LocationId, ids.Location2Id, "LineHaul", TimeSpan.FromHours(8), true));
        
        var request = new PagedRequest { Page = 1, PageSize = 10 };
        var result = await service.GetByOriginAsync(ids.LocationId, request);

        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
    }

    [Fact]
    public async Task ExistsAsync_NonExistingLink_ReturnsFalse()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new NetworkLinkService(uow);

        var exists = await service.ExistsAsync(Guid.NewGuid());

        Assert.False(exists);
    }
}
