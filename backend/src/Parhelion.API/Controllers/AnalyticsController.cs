using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Services;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controller for ML Analytics endpoints.
/// Orchestrates calls to Python Analytics internal service.
/// </summary>
[ApiController]
[Route("api/analytics")]
[Authorize]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IPythonAnalyticsClient _pythonClient;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IPythonAnalyticsClient pythonClient,
        ICurrentUserService currentUser,
        ILogger<AnalyticsController> logger)
    {
        _pythonClient = pythonClient;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Optimize route between two locations using graph algorithms.
    /// </summary>
    [HttpPost("routes/optimize")]
    public async Task<ActionResult<RouteOptimizeResponse>> OptimizeRoute(
        [FromBody] RouteOptimizeApiRequest request,
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized();

        _logger.LogInformation("Route optimize: {Origin} -> {Dest}", request.OriginId, request.DestinationId);

        var result = await _pythonClient.OptimizeRouteAsync(new RouteOptimizeRequest(
            tenantId.Value,
            request.OriginId,
            request.DestinationId,
            request.AvoidLocations,
            request.MaxTimeHours
        ), ct);

        return Ok(result);
    }

    /// <summary>
    /// Recommend trucks for a shipment using ML scoring.
    /// </summary>
    [HttpGet("trucks/recommend/{shipmentId}")]
    public async Task<ActionResult<List<TruckRecommendation>>> RecommendTrucks(
        string shipmentId,
        [FromQuery] int limit = 3,
        [FromQuery] bool considerDeadhead = true,
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized();

        _logger.LogInformation("Truck recommend for shipment: {ShipmentId}", shipmentId);

        var result = await _pythonClient.RecommendTrucksAsync(new TruckRecommendRequest(
            tenantId.Value,
            shipmentId,
            limit,
            considerDeadhead
        ), ct);

        return Ok(result);
    }

    /// <summary>
    /// Forecast shipment demand using time series analysis.
    /// </summary>
    [HttpGet("forecast/demand")]
    public async Task<ActionResult<DemandForecastResponse>> ForecastDemand(
        [FromQuery] string? locationId = null,
        [FromQuery] int days = 30,
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized();

        _logger.LogInformation("Demand forecast for {Days} days", days);

        var result = await _pythonClient.ForecastDemandAsync(new DemandForecastRequest(
            tenantId.Value,
            locationId,
            days
        ), ct);

        return Ok(result);
    }

    /// <summary>
    /// Detect anomalies in shipment tracking.
    /// </summary>
    [HttpGet("anomalies")]
    public async Task<ActionResult<List<AnomalyAlert>>> DetectAnomalies(
        [FromQuery] int hoursBack = 24,
        [FromQuery] string? severity = null,
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized();

        _logger.LogInformation("Anomaly detection for last {Hours} hours", hoursBack);

        var result = await _pythonClient.DetectAnomaliesAsync(new AnomalyDetectRequest(
            tenantId.Value,
            hoursBack,
            severity
        ), ct);

        return Ok(result);
    }

    /// <summary>
    /// Optimize 3D cargo loading for a truck.
    /// </summary>
    [HttpPost("loading/optimize")]
    public async Task<ActionResult<LoadingOptimizeResponse>> OptimizeLoading(
        [FromBody] LoadingOptimizeApiRequest request,
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized();

        _logger.LogInformation("Loading optimize for truck: {TruckId}", request.TruckId);

        var result = await _pythonClient.OptimizeLoadingAsync(new LoadingOptimizeRequest(
            tenantId.Value,
            request.TruckId,
            request.ShipmentIds
        ), ct);

        return Ok(result);
    }

    /// <summary>
    /// Generate operational dashboard with KPIs.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardResponse>> GetDashboard(
        [FromQuery] bool refresh = false,
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized();

        _logger.LogInformation("Dashboard generation requested");

        var result = await _pythonClient.GenerateDashboardAsync(new DashboardRequest(
            tenantId.Value,
            refresh
        ), ct);

        return Ok(result);
    }

    /// <summary>
    /// Analyze logistics network topology.
    /// </summary>
    [HttpGet("network")]
    public async Task<ActionResult<NetworkAnalysisResponse>> AnalyzeNetwork(
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized();

        _logger.LogInformation("Network analysis requested");

        var result = await _pythonClient.AnalyzeNetworkAsync(new NetworkAnalyzeRequest(
            tenantId.Value
        ), ct);

        return Ok(result);
    }

    /// <summary>
    /// Cluster shipments geographically for route consolidation.
    /// </summary>
    [HttpPost("shipments/cluster")]
    public async Task<ActionResult<List<ShipmentCluster>>> ClusterShipments(
        [FromBody] ShipmentClusterApiRequest request,
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized();

        _logger.LogInformation("Clustering into {Count} groups", request.ClusterCount);

        var result = await _pythonClient.ClusterShipmentsAsync(new ShipmentClusterRequest(
            tenantId.Value,
            request.ClusterCount,
            request.DateFrom,
            request.DateTo
        ), ct);

        return Ok(result);
    }

    /// <summary>
    /// Predict ETA for a shipment using ML.
    /// </summary>
    [HttpGet("eta/{shipmentId}")]
    public async Task<ActionResult<ETAPredictionResponse>> PredictETA(
        string shipmentId,
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized();

        _logger.LogInformation("ETA prediction for: {ShipmentId}", shipmentId);

        var result = await _pythonClient.PredictETAAsync(new ETAPredictRequest(
            tenantId.Value,
            shipmentId
        ), ct);

        return Ok(result);
    }

    /// <summary>
    /// Get driver performance metrics.
    /// </summary>
    [HttpGet("drivers/{driverId}/performance")]
    public async Task<ActionResult<DriverPerformanceResponse>> GetDriverPerformance(
        string driverId,
        [FromQuery] int days = 30,
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized();

        _logger.LogInformation("Performance for driver: {DriverId}", driverId);

        var result = await _pythonClient.GetDriverPerformanceAsync(new DriverPerformanceRequest(
            tenantId.Value,
            driverId,
            days
        ), ct);

        return Ok(result);
    }

    /// <summary>
    /// Get driver leaderboard.
    /// </summary>
    [HttpGet("drivers/leaderboard")]
    public async Task<ActionResult<List<LeaderboardEntry>>> GetLeaderboard(
        [FromQuery] int limit = 10,
        [FromQuery] int days = 30,
        CancellationToken ct = default)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized();

        _logger.LogInformation("Driver leaderboard, top {Limit}", limit);

        var result = await _pythonClient.GetLeaderboardAsync(new LeaderboardRequest(
            tenantId.Value,
            limit,
            days
        ), ct);

        return Ok(result);
    }

    private Guid? GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return tenantId;
        return null;
    }
}

// ============ API Request DTOs (simplified for clients) ============

public record RouteOptimizeApiRequest(
    string OriginId,
    string DestinationId,
    List<string>? AvoidLocations = null,
    double? MaxTimeHours = null
);

public record LoadingOptimizeApiRequest(
    string TruckId,
    List<string> ShipmentIds
);

public record ShipmentClusterApiRequest(
    int ClusterCount = 5,
    string? DateFrom = null,
    string? DateTo = null
);
