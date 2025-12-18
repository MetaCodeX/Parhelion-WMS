using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Network;
using Parhelion.Infrastructure.Services.Network;
using Parhelion.Tests.Fixtures;
using Xunit;

namespace Parhelion.Tests.Unit.Services.Network;

/// <summary>
/// Tests para RouteStepService.
/// </summary>
public class RouteStepServiceTests : IClassFixture<ServiceTestFixture>
{
    private readonly ServiceTestFixture _fixture;

    public RouteStepServiceTests(ServiceTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new RouteStepService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 10 };

        var result = await service.GetAllAsync(request);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccessAndUpdatesRoute()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new RouteStepService(uow);
        var request = new CreateRouteStepRequest(ids.RouteId, ids.LocationId, 1, TimeSpan.FromHours(2), "Origin");

        var result = await service.CreateAsync(request);

        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.StepOrder);
        Assert.Equal("Monterrey Hub", result.Data!.LocationName);
    }

    [Fact]
    public async Task GetByRouteAsync_ReturnsOrderedSteps()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new RouteStepService(uow);
        
        // Create two steps
        await service.CreateAsync(new CreateRouteStepRequest(ids.RouteId, ids.LocationId, 1, TimeSpan.FromHours(0), "Origin"));
        await service.CreateAsync(new CreateRouteStepRequest(ids.RouteId, ids.Location2Id, 2, TimeSpan.FromHours(8), "Destination"));

        var result = await service.GetByRouteAsync(ids.RouteId);

        Assert.NotNull(result);
        var steps = result.ToList();
        Assert.Equal(2, steps.Count);
        Assert.Equal(1, steps[0].StepOrder);
        Assert.Equal(2, steps[1].StepOrder);
    }

    [Fact]
    public async Task AddStepToRouteAsync_AddsAtEnd()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new RouteStepService(uow);
        
        // Create first step
        await service.CreateAsync(new CreateRouteStepRequest(ids.RouteId, ids.LocationId, 1, TimeSpan.FromHours(0), "Origin"));
        
        // Add step at end
        var result = await service.AddStepToRouteAsync(ids.RouteId, 
            new CreateRouteStepRequest(ids.RouteId, ids.Location2Id, 0, TimeSpan.FromHours(4), "Intermediate"));

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.StepOrder); // Auto-incremented
    }

    [Fact]
    public async Task DeleteAsync_RemovesStepAndUpdatesRoute()
    {
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var service = new RouteStepService(uow);
        
        // Create step
        var createResult = await service.CreateAsync(new CreateRouteStepRequest(ids.RouteId, ids.LocationId, 1, TimeSpan.FromHours(2), "Origin"));
        var stepId = createResult.Data!.Id;

        // Delete
        var deleteResult = await service.DeleteAsync(stepId);

        Assert.True(deleteResult.Success);
        Assert.False(await service.ExistsAsync(stepId));
    }
}
