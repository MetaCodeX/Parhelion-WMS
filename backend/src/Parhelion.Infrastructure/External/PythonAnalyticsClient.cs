using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Parhelion.Application.Interfaces;

namespace Parhelion.Infrastructure.External;

/// <summary>
/// HTTP client for Python Analytics internal service.
/// Configured with Polly retry policies for resilience.
/// </summary>
public class PythonAnalyticsClient : IPythonAnalyticsClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public PythonAnalyticsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
    }

    public async Task<RouteOptimizeResponse> OptimizeRouteAsync(
        RouteOptimizeRequest request, CancellationToken ct = default)
    {
        var pythonRequest = new
        {
            tenant_id = request.TenantId,
            origin_id = request.OriginId,
            destination_id = request.DestinationId,
            avoid_locations = request.AvoidLocations ?? new List<string>(),
            max_time_hours = request.MaxTimeHours
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/internal/routes/optimize", pythonRequest, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<RouteOptimizeResponse>(_jsonOptions, ct)
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<List<TruckRecommendation>> RecommendTrucksAsync(
        TruckRecommendRequest request, CancellationToken ct = default)
    {
        var pythonRequest = new
        {
            tenant_id = request.TenantId,
            shipment_id = request.ShipmentId,
            limit = request.Limit,
            consider_deadhead = request.ConsiderDeadhead
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/internal/trucks/recommend", pythonRequest, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<TruckRecommendation>>(_jsonOptions, ct)
            ?? new List<TruckRecommendation>();
    }

    public async Task<DemandForecastResponse> ForecastDemandAsync(
        DemandForecastRequest request, CancellationToken ct = default)
    {
        var pythonRequest = new
        {
            tenant_id = request.TenantId,
            location_id = request.LocationId,
            days = request.Days
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/internal/forecast/demand", pythonRequest, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<DemandForecastResponse>(_jsonOptions, ct)
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<List<AnomalyAlert>> DetectAnomaliesAsync(
        AnomalyDetectRequest request, CancellationToken ct = default)
    {
        var pythonRequest = new
        {
            tenant_id = request.TenantId,
            hours_back = request.HoursBack,
            severity_filter = request.SeverityFilter
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/internal/anomalies/detect", pythonRequest, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<AnomalyAlert>>(_jsonOptions, ct)
            ?? new List<AnomalyAlert>();
    }

    public async Task<LoadingOptimizeResponse> OptimizeLoadingAsync(
        LoadingOptimizeRequest request, CancellationToken ct = default)
    {
        var pythonRequest = new
        {
            tenant_id = request.TenantId,
            truck_id = request.TruckId,
            shipment_ids = request.ShipmentIds
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/internal/loading/optimize", pythonRequest, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<LoadingOptimizeResponse>(_jsonOptions, ct)
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<DashboardResponse> GenerateDashboardAsync(
        DashboardRequest request, CancellationToken ct = default)
    {
        var pythonRequest = new
        {
            tenant_id = request.TenantId,
            refresh_cache = request.RefreshCache
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/internal/dashboard/generate", pythonRequest, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<DashboardResponse>(_jsonOptions, ct)
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<NetworkAnalysisResponse> AnalyzeNetworkAsync(
        NetworkAnalyzeRequest request, CancellationToken ct = default)
    {
        var pythonRequest = new { tenant_id = request.TenantId };

        var response = await _httpClient.PostAsJsonAsync(
            "/internal/network/analyze", pythonRequest, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<NetworkAnalysisResponse>(_jsonOptions, ct)
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<List<ShipmentCluster>> ClusterShipmentsAsync(
        ShipmentClusterRequest request, CancellationToken ct = default)
    {
        var pythonRequest = new
        {
            tenant_id = request.TenantId,
            cluster_count = request.ClusterCount,
            date_from = request.DateFrom,
            date_to = request.DateTo
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/internal/shipments/cluster", pythonRequest, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<ShipmentCluster>>(_jsonOptions, ct)
            ?? new List<ShipmentCluster>();
    }

    public async Task<ETAPredictionResponse> PredictETAAsync(
        ETAPredictRequest request, CancellationToken ct = default)
    {
        var pythonRequest = new
        {
            tenant_id = request.TenantId,
            shipment_id = request.ShipmentId
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/internal/eta/predict", pythonRequest, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<ETAPredictionResponse>(_jsonOptions, ct)
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<DriverPerformanceResponse> GetDriverPerformanceAsync(
        DriverPerformanceRequest request, CancellationToken ct = default)
    {
        var pythonRequest = new
        {
            tenant_id = request.TenantId,
            driver_id = request.DriverId,
            days_back = request.DaysBack
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/internal/drivers/performance", pythonRequest, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<DriverPerformanceResponse>(_jsonOptions, ct)
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(
        LeaderboardRequest request, CancellationToken ct = default)
    {
        var pythonRequest = new
        {
            tenant_id = request.TenantId,
            limit = request.Limit,
            days_back = request.DaysBack
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/internal/drivers/leaderboard", pythonRequest, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<LeaderboardEntry>>(_jsonOptions, ct)
            ?? new List<LeaderboardEntry>();
    }
}
