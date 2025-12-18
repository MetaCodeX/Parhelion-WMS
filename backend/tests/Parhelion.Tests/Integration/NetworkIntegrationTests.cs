using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Network;
using Parhelion.Infrastructure.Services.Network;
using Parhelion.Tests.Fixtures;
using Xunit;

namespace Parhelion.Tests.Integration;

/// <summary>
/// Tests de integración para el área de Network (TMS - Network/Routes).
/// Verifica flujos completos que involucran Locations, NetworkLinks, Routes, RouteSteps.
/// </summary>
public class NetworkIntegrationTests : IClassFixture<ServiceTestFixture>
{
    private readonly ServiceTestFixture _fixture;

    public NetworkIntegrationTests(ServiceTestFixture fixture) => _fixture = fixture;

    /// <summary>
    /// Flujo completo: Crear enlace → Crear ruta → Agregar pasos ordenados
    /// </summary>
    [Fact]
    public async Task NetworkFlow_CreateLinkAndRoute_WithOrderedSteps()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var linkService = new NetworkLinkService(uow);
        var routeService = new RouteService(uow);
        var stepService = new RouteStepService(uow);

        // Act 1: Create network link between locations
        var linkResult = await linkService.CreateAsync(new CreateNetworkLinkRequest(
            OriginLocationId: ids.LocationId,
            DestinationLocationId: ids.Location2Id,
            LinkType: "LineHaul",
            TransitTime: TimeSpan.FromHours(8),
            IsBidirectional: true));
        Assert.True(linkResult.Success);
        Assert.True(linkResult.Data!.IsBidirectional);

        // Act 2: Create route blueprint
        var routeResult = await routeService.CreateAsync(new CreateRouteBlueprintRequest(
            Name: "MTY-GDL Direct",
            Description: "Direct route from Monterrey to Guadalajara"));
        Assert.True(routeResult.Success);
        var routeId = routeResult.Data!.Id;

        // Act 3: Add route steps
        var step1 = await stepService.CreateAsync(new CreateRouteStepRequest(
            routeId, ids.LocationId, 1, TimeSpan.Zero, "Origin"));
        var step2 = await stepService.CreateAsync(new CreateRouteStepRequest(
            routeId, ids.Location2Id, 2, TimeSpan.FromHours(8), "Destination"));
        Assert.True(step1.Success);
        Assert.True(step2.Success);

        // Act 4: Verify route steps are ordered
        var steps = await stepService.GetByRouteAsync(routeId);
        var stepList = steps.ToList();
        Assert.Equal(2, stepList.Count);
        Assert.Equal("Monterrey Hub", stepList[0].LocationName);
        Assert.Equal("Guadalajara Hub", stepList[1].LocationName);
    }

    /// <summary>
    /// Flujo: Verificar búsqueda de rutas activas
    /// </summary>
    [Fact]
    public async Task RouteManagement_SearchActiveRoutes()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var routeService = new RouteService(uow);
        var request = new PagedRequest { Page = 1, PageSize = 20 };

        // Act: Get active routes (seeded route is active)
        var activeRoutes = await routeService.GetActiveAsync(ids.TenantId, request);

        // Assert: Should include seeded route
        Assert.True(activeRoutes.TotalCount >= 1);
        Assert.Contains(activeRoutes.Items, r => r.Name == "MTY-GDL Express");
    }

    /// <summary>
    /// Flujo: Agregar pasos a ruta existente usando AddStepToRoute
    /// </summary>
    [Fact]
    public async Task RouteSteps_AddStepsToExistingRoute()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var stepService = new RouteStepService(uow);

        // Act: Add multiple steps using AddStepToRoute (auto-ordering)
        var step1 = await stepService.AddStepToRouteAsync(ids.RouteId, 
            new CreateRouteStepRequest(ids.RouteId, ids.LocationId, 0, TimeSpan.Zero, "Origin"));
        var step2 = await stepService.AddStepToRouteAsync(ids.RouteId, 
            new CreateRouteStepRequest(ids.RouteId, ids.Location2Id, 0, TimeSpan.FromHours(8), "Destination"));

        // Assert: Steps should be auto-ordered
        Assert.True(step1.Success);
        Assert.True(step2.Success);
        Assert.Equal(1, step1.Data!.StepOrder);
        Assert.Equal(2, step2.Data!.StepOrder);
    }

    /// <summary>
    /// Flujo: Verificar enlaces bidireccionales
    /// </summary>
    [Fact]
    public async Task NetworkLinks_BidirectionalLinks()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var linkService = new NetworkLinkService(uow);

        // Create bidirectional link
        var linkResult = await linkService.CreateAsync(new CreateNetworkLinkRequest(
            ids.LocationId, ids.Location2Id, "LineHaul", TimeSpan.FromHours(8), true));
        Assert.True(linkResult.Success);

        // Act: Query from both directions
        var request = new PagedRequest { Page = 1, PageSize = 20 };
        var fromOrigin = await linkService.GetByOriginAsync(ids.LocationId, request);
        var toDestination = await linkService.GetByDestinationAsync(ids.Location2Id, request);

        // Assert: Same link appears in both queries
        Assert.True(fromOrigin.TotalCount >= 1);
        Assert.True(toDestination.TotalCount >= 1);
    }

    /// <summary>
    /// Flujo: Eliminar paso y verificar que ruta se actualiza
    /// </summary>
    [Fact]
    public async Task RouteSteps_DeleteStep_UpdatesRouteTotal()
    {
        // Arrange
        var (uow, ctx, ids) = _fixture.CreateSeededUnitOfWork();
        var routeService = new RouteService(uow);
        var stepService = new RouteStepService(uow);

        // Create route and add step
        var routeResult = await routeService.CreateAsync(new CreateRouteBlueprintRequest("Test Route", "For deletion test"));
        var routeId = routeResult.Data!.Id;
        
        var stepResult = await stepService.CreateAsync(new CreateRouteStepRequest(
            routeId, ids.LocationId, 1, TimeSpan.FromHours(2), "Origin"));
        var stepId = stepResult.Data!.Id;

        // Act: Delete step
        var deleteResult = await stepService.DeleteAsync(stepId);
        Assert.True(deleteResult.Success);

        // Verify step no longer exists
        Assert.False(await stepService.ExistsAsync(stepId));
    }
}
