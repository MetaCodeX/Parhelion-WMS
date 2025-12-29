using System.Text.Json;

namespace Parhelion.Application.Interfaces;

/// <summary>
/// Interface for Python Analytics internal service client.
/// Provides access to ML-based analytics capabilities.
/// </summary>
public interface IPythonAnalyticsClient
{
    /// <summary>
    /// Optimize route between two locations using graph algorithms.
    /// </summary>
    Task<RouteOptimizeResponse> OptimizeRouteAsync(RouteOptimizeRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Recommend trucks for a shipment based on ML scoring.
    /// </summary>
    Task<List<TruckRecommendation>> RecommendTrucksAsync(TruckRecommendRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Forecast shipment demand using time series analysis.
    /// </summary>
    Task<DemandForecastResponse> ForecastDemandAsync(DemandForecastRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Detect anomalies in shipment tracking using Isolation Forest.
    /// </summary>
    Task<List<AnomalyAlert>> DetectAnomaliesAsync(AnomalyDetectRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Optimize 3D cargo loading for a truck.
    /// </summary>
    Task<LoadingOptimizeResponse> OptimizeLoadingAsync(LoadingOptimizeRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Generate operational dashboard with KPIs.
    /// </summary>
    Task<DashboardResponse> GenerateDashboardAsync(DashboardRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Analyze logistics network topology.
    /// </summary>
    Task<NetworkAnalysisResponse> AnalyzeNetworkAsync(NetworkAnalyzeRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Cluster shipments geographically for route consolidation.
    /// </summary>
    Task<List<ShipmentCluster>> ClusterShipmentsAsync(ShipmentClusterRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Predict ETA using ML model.
    /// </summary>
    Task<ETAPredictionResponse> PredictETAAsync(ETAPredictRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Get driver performance metrics.
    /// </summary>
    Task<DriverPerformanceResponse> GetDriverPerformanceAsync(DriverPerformanceRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Get driver leaderboard.
    /// </summary>
    Task<List<LeaderboardEntry>> GetLeaderboardAsync(LeaderboardRequest request, CancellationToken ct = default);
}

// ============ Request DTOs ============

public record RouteOptimizeRequest(
    Guid TenantId,
    string OriginId,
    string DestinationId,
    List<string>? AvoidLocations = null,
    double? MaxTimeHours = null
);

public record TruckRecommendRequest(
    Guid TenantId,
    string ShipmentId,
    int Limit = 3,
    bool ConsiderDeadhead = true
);

public record DemandForecastRequest(
    Guid TenantId,
    string? LocationId = null,
    int Days = 30
);

public record AnomalyDetectRequest(
    Guid TenantId,
    int HoursBack = 24,
    string? SeverityFilter = null
);

public record LoadingOptimizeRequest(
    Guid TenantId,
    string TruckId,
    List<string> ShipmentIds
);

public record DashboardRequest(
    Guid TenantId,
    bool RefreshCache = false
);

public record NetworkAnalyzeRequest(
    Guid TenantId
);

public record ShipmentClusterRequest(
    Guid TenantId,
    int ClusterCount = 5,
    string? DateFrom = null,
    string? DateTo = null
);

public record ETAPredictRequest(
    Guid TenantId,
    string ShipmentId
);

public record DriverPerformanceRequest(
    Guid TenantId,
    string DriverId,
    int DaysBack = 30
);

public record LeaderboardRequest(
    Guid TenantId,
    int Limit = 10,
    int DaysBack = 30
);

// ============ Response DTOs ============

public record RouteOptimizeResponse(
    List<RoutePathNode> OptimalPath,
    double TotalTimeHours,
    double TotalDistanceKm,
    int Hops,
    string Algorithm,
    double ConfidenceScore,
    List<AlternativeRoute> Alternatives,
    List<Bottleneck> Bottlenecks
);

public record RoutePathNode(string Id, string Code, string Name, string Type);
public record AlternativeRoute(int Rank, List<string> Path, double TimeHours, int Hops);
public record Bottleneck(string LocationId, string Code, string Name, double CentralityScore, string Severity);

public record TruckRecommendation(
    TruckInfo Truck,
    double Score,
    double ProjectedUtilization,
    double DeadheadKm,
    List<string> Reasons,
    bool Compatible
);

public record TruckInfo(string Id, string Plate, string Type, double MaxCapacityKg, double CurrentLoadKg, double AvailableKg);

public record DemandForecastResponse(
    Guid TenantId,
    string? LocationId,
    int ForecastPeriodDays,
    string GeneratedAt,
    List<DayPrediction> Predictions,
    List<string> PeakDays,
    ForecastStatistics Statistics,
    ResourceRecommendations ResourceRecommendations
);

public record DayPrediction(string Date, int PredictedShipments, int LowerBound, int UpperBound, double Confidence, bool IsHoliday, string DayOfWeek);
public record ForecastStatistics(double AvgDailyShipments, int MaxDailyShipments, int TotalPredicted);
public record ResourceRecommendations(int RecommendedTrucks, int RecommendedDrivers, int PeakDayTrucks);

public record AnomalyAlert(
    string Id,
    string ShipmentId,
    string TrackingNumber,
    string DetectedAt,
    string AnomalyType,
    string Severity,
    double AnomalyScore,
    string Description,
    List<string> SuggestedActions
);

public record LoadingOptimizeResponse(
    string TruckId,
    string TruckPlate,
    int LoadedItemsCount,
    int UnfittedItemsCount,
    List<LoadedItem> Items,
    UtilizationInfo Utilization,
    WeightDistribution WeightDistribution,
    List<LoadingStep> LoadingSequence,
    List<string> Warnings
);

public record LoadedItem(ItemInfo Item, Position3D Position);
public record ItemInfo(string Sku, string Description, double WeightKg, bool IsFragile);
public record Position3D(double X, double Y, double Z);
public record UtilizationInfo(double VolumeRate, double WeightRate, double TotalWeightKg, double TotalVolumeM3);
public record WeightDistribution(CenterOfGravity CenterOfGravity, bool IsBalanced, double FrontWeightPct, double RearWeightPct);
public record CenterOfGravity(double X, double Y, double Z, double FrontPct, double RearPct);
public record LoadingStep(int Step, string Sku, string Description, string Position);

public record DashboardResponse(
    Guid TenantId,
    string GeneratedAt,
    string CacheStatus,
    DashboardKpis Kpis,
    TimeSeriesData TimeSeries,
    RouteAnalytics RouteAnalytics,
    List<CongestionHotspot> CongestionHotspots
);

public record DashboardKpis(
    int TotalShipmentsToday,
    double OnTimeDeliveryRate,
    double AvgTruckUtilization,
    int ShipmentsAtRisk,
    double AvgTransitTimeHours,
    int ActiveDrivers,
    int IdleDrivers,
    int IdleTrucks,
    int InTransitShipments
);

public record TimeSeriesData(List<TimeSeriesPoint> ShipmentsOverTime, List<StatusDistribution> StatusDistribution);
public record TimeSeriesPoint(string Date, int Value);
public record StatusDistribution(string Status, int Count);
public record RouteAnalytics(List<TopRoute> TopRoutes, List<ProblemRoute> ProblemRoutes);
public record TopRoute(string Origin, string Destination, int ShipmentCount, double AvgTransitHours, double DelayRate);
public record ProblemRoute(string Origin, string Destination, double DelayRate);
public record CongestionHotspot(string LocationId, int WaitingShipments, string Severity);

public record NetworkAnalysisResponse(
    Guid TenantId,
    BasicMetrics BasicMetrics,
    List<CriticalHub> CriticalHubs,
    List<Community> Communities,
    List<CriticalPath> CriticalPaths,
    ResilienceAnalysis ResilienceAnalysis,
    List<NetworkBottleneck> Bottlenecks
);

public record BasicMetrics(int TotalNodes, int TotalEdges, double NetworkDensity, double AveragePathLength);
public record CriticalHub(string LocationId, string Code, string Name, double CentralityScore, double Closeness);
public record Community(int Id, List<string> Locations, int Size, double Density);
public record CriticalPath(string Origin, string Destination, List<string> Path, int Hops, double Distance);
public record ResilienceAnalysis(CriticalHubInfo MostCriticalHub, bool IsConnectedAfterRemoval, int ComponentsAfterRemoval, List<string> IsolatedLocations, double ResilienceScore);
public record CriticalHubInfo(string Id, string Name);
public record NetworkBottleneck(string LocationId, string Code, string Name, double Centrality, string Severity);

public record ShipmentCluster(
    int ClusterId,
    ClusterCentroid Centroid,
    List<ClusterShipment> Shipments,
    int ShipmentCount,
    double TotalWeightKg,
    double TotalVolumeM3,
    RecommendedHub RecommendedHub,
    string RecommendedTruckType,
    RouteOptimization RouteOptimization
);

public record ClusterCentroid(double Latitude, double Longitude);
public record ClusterShipment(string Id, string TrackingNumber, string RecipientName, double WeightKg, string Destination);
public record RecommendedHub(string Code, string Name, double DistanceKm);
public record RouteOptimization(double IndividualKm, double OptimizedKm, double SavingsKm, double SavingsPct);

public record ETAPredictionResponse(
    string ShipmentId,
    string TrackingNumber,
    string BaseETA,
    string PredictedETA,
    double PredictedDelayHours,
    string ConfidenceLower,
    string ConfidenceUpper,
    double ConfidenceScore,
    ETAFactors Factors,
    string Status
);

public record ETAFactors(
    DriverPerformanceImpact DriverPerformance,
    RouteReliability RouteReliability,
    TrafficConditions TrafficConditions,
    LoadFactor LoadFactor
);

public record DriverPerformanceImpact(double AvgDelayMinutes, string Impact);
public record RouteReliability(double DelayRatePct, string Impact);
public record TrafficConditions(bool IsRushHour, string Impact);
public record LoadFactor(double WeightPct, string Impact);

public record DriverPerformanceResponse(
    string DriverId,
    string DriverName,
    int AnalysisPeriodDays,
    string GeneratedAt,
    PerformanceMetrics Metrics,
    Rating Rating,
    Comparison Comparison,
    Trend Trend,
    List<string> Recommendations
);

public record PerformanceMetrics(int TotalDeliveries, double OnTimeRate, double AvgDelayMinutes, int ExceptionCount, int MissingCheckpoints);
public record Rating(double Stars, string Category);
public record Comparison(int PercentileRank, bool IsTopPerformer, double VsAverage);
public record Trend(string Direction, double ChangePct);

public record LeaderboardEntry(
    int Rank,
    string DriverId,
    string DriverName,
    double OnTimeRate,
    double AvgDelayMinutes,
    int TotalDeliveries,
    double Rating
);
