"""
Unit Tests for ML Analytics Services (Modules 1-10)

Tests the 10 Python analytics modules:
1. RouteOptimizer
2. TruckRecommender
3. DemandForecaster
4. AnomalyDetector
5. LoadingOptimizer
6. AnalyticsEngine
7. NetworkAnalyzer
8. ShipmentClusterer
9. ETAPredictor
10. DriverPerformanceAnalyzer
"""

import pytest
from uuid import uuid4

from parhelion_py.application.services import (
    RouteOptimizer,
    TruckRecommender,
    DemandForecaster,
    AnomalyDetector,
    LoadingOptimizer,
    AnalyticsEngine,
    NetworkAnalyzer,
    ShipmentClusterer,
    ETAPredictor,
    DriverPerformanceAnalyzer,
)


# ============ Module 1: Route Optimizer ============

class TestRouteOptimizer:
    """Tests for Route Optimizer service."""

    @pytest.fixture
    def optimizer(self):
        return RouteOptimizer()

    @pytest.fixture
    def tenant_id(self):
        return uuid4()

    @pytest.mark.asyncio
    async def test_calculate_optimal_route_success(self, optimizer, tenant_id):
        """Test successful route calculation."""
        result = await optimizer.calculate_optimal_route(
            tenant_id=tenant_id,
            origin_id="MTY-HUB",
            destination_id="CDMX-HUB",
        )
        
        assert "optimal_path" in result
        assert "total_time_hours" in result
        assert "alternatives" in result
        assert len(result["optimal_path"]) >= 2

    @pytest.mark.asyncio
    async def test_calculate_route_with_constraints(self, optimizer, tenant_id):
        """Test route calculation with constraints."""
        result = await optimizer.calculate_optimal_route(
            tenant_id=tenant_id,
            origin_id="MTY-HUB",
            destination_id="GDL-HUB",
            constraints={"avoid_locations": ["CDMX-HUB"]},
        )
        
        assert "optimal_path" in result
        # CDMX should not be in path
        path_ids = [p.get("id", p) for p in result.get("optimal_path", [])]
        assert "CDMX-HUB" not in path_ids

    @pytest.mark.asyncio
    async def test_invalid_origin(self, optimizer, tenant_id):
        """Test with invalid origin."""
        result = await optimizer.calculate_optimal_route(
            tenant_id=tenant_id,
            origin_id="INVALID-LOC",
            destination_id="CDMX-HUB",
        )
        
        assert "error" in result


# ============ Module 2: Truck Recommender ============

class TestTruckRecommender:
    """Tests for Truck Recommender service."""

    @pytest.fixture
    def recommender(self):
        return TruckRecommender()

    @pytest.fixture
    def tenant_id(self):
        return uuid4()

    @pytest.mark.asyncio
    async def test_recommend_trucks(self, recommender, tenant_id):
        """Test truck recommendations."""
        result = await recommender.recommend_trucks(
            tenant_id=tenant_id,
            shipment_id="ship-001",
            limit=3,
        )
        
        assert isinstance(result, list)
        assert len(result) <= 3
        
        if result:
            assert "truck" in result[0]
            assert "score" in result[0]
            assert "compatible" in result[0]

    @pytest.mark.asyncio
    async def test_recommend_trucks_with_deadhead(self, recommender, tenant_id):
        """Test recommendations considering deadhead."""
        result = await recommender.recommend_trucks(
            tenant_id=tenant_id,
            shipment_id="ship-002",
            consider_deadhead=True,
        )
        
        assert isinstance(result, list)
        if result:
            assert "deadhead_km" in result[0]


# ============ Module 3: Demand Forecaster ============

class TestDemandForecaster:
    """Tests for Demand Forecaster service."""

    @pytest.fixture
    def forecaster(self):
        return DemandForecaster()

    @pytest.fixture
    def tenant_id(self):
        return uuid4()

    @pytest.mark.asyncio
    async def test_forecast_demand(self, forecaster, tenant_id):
        """Test demand forecasting."""
        result = await forecaster.forecast_demand(
            tenant_id=tenant_id,
            days=7,
        )
        
        assert "predictions" in result
        assert "peak_days" in result
        assert "resource_recommendations" in result
        assert len(result["predictions"]) == 7

    @pytest.mark.asyncio
    async def test_forecast_with_location(self, forecaster, tenant_id):
        """Test forecasting for specific location."""
        result = await forecaster.forecast_demand(
            tenant_id=tenant_id,
            location_id="MTY-HUB",
            days=14,
        )
        
        assert result["location_id"] == "MTY-HUB"
        assert len(result["predictions"]) == 14


# ============ Module 4: Anomaly Detector ============

class TestAnomalyDetector:
    """Tests for Anomaly Detector service."""

    @pytest.fixture
    def detector(self):
        return AnomalyDetector()

    @pytest.fixture
    def tenant_id(self):
        return uuid4()

    @pytest.mark.asyncio
    async def test_detect_anomalies(self, detector, tenant_id):
        """Test anomaly detection."""
        result = await detector.detect_anomalies(
            tenant_id=tenant_id,
            hours_back=24,
        )
        
        assert isinstance(result, list)
        
        if result:
            assert "anomaly_type" in result[0]
            assert "severity" in result[0]
            assert "suggested_actions" in result[0]

    @pytest.mark.asyncio
    async def test_filter_by_severity(self, detector, tenant_id):
        """Test filtering by severity."""
        result = await detector.detect_anomalies(
            tenant_id=tenant_id,
            severity_filter="CRITICAL",
        )
        
        for anomaly in result:
            assert anomaly["severity"] == "CRITICAL"


# ============ Module 5: Loading Optimizer ============

class TestLoadingOptimizer:
    """Tests for Loading Optimizer service."""

    @pytest.fixture
    def optimizer(self):
        return LoadingOptimizer()

    @pytest.fixture
    def tenant_id(self):
        return uuid4()

    @pytest.mark.asyncio
    async def test_optimize_loading(self, optimizer, tenant_id):
        """Test 3D loading optimization."""
        result = await optimizer.optimize_loading(
            tenant_id=tenant_id,
            truck_id="truck-001",
            shipment_ids=["ship-001", "ship-002"],
        )
        
        assert "items" in result
        assert "utilization" in result
        assert "weight_distribution" in result
        assert "loading_sequence" in result

    @pytest.mark.asyncio
    async def test_loading_stability(self, optimizer, tenant_id):
        """Test weight distribution analysis."""
        result = await optimizer.optimize_loading(
            tenant_id=tenant_id,
            truck_id="truck-001",
            shipment_ids=["ship-001"],
        )
        
        assert "weight_distribution" in result
        assert "is_balanced" in result["weight_distribution"]


# ============ Module 6: Analytics Engine ============

class TestAnalyticsEngine:
    """Tests for Analytics Engine service."""

    @pytest.fixture
    def engine(self):
        return AnalyticsEngine()

    @pytest.fixture
    def tenant_id(self):
        return uuid4()

    @pytest.mark.asyncio
    async def test_generate_dashboard(self, engine, tenant_id):
        """Test dashboard generation."""
        result = await engine.generate_dashboard(tenant_id=tenant_id)
        
        assert "kpis" in result
        assert "time_series" in result
        assert "route_analytics" in result

    @pytest.mark.asyncio
    async def test_kpis_content(self, engine, tenant_id):
        """Test KPI calculations."""
        result = await engine.generate_dashboard(tenant_id=tenant_id)
        
        kpis = result["kpis"]
        assert "on_time_delivery_rate" in kpis
        assert "avg_truck_utilization" in kpis
        assert "shipments_at_risk" in kpis


# ============ Module 7: Network Analyzer ============

class TestNetworkAnalyzer:
    """Tests for Network Analyzer service."""

    @pytest.fixture
    def analyzer(self):
        return NetworkAnalyzer()

    @pytest.fixture
    def tenant_id(self):
        return uuid4()

    @pytest.mark.asyncio
    async def test_analyze_network(self, analyzer, tenant_id):
        """Test network analysis."""
        result = await analyzer.analyze_network(tenant_id=tenant_id)
        
        assert "basic_metrics" in result
        assert "critical_hubs" in result
        assert "resilience_analysis" in result

    @pytest.mark.asyncio
    async def test_critical_hubs(self, analyzer, tenant_id):
        """Test critical hub identification."""
        result = await analyzer.analyze_network(tenant_id=tenant_id)
        
        assert len(result["critical_hubs"]) > 0
        assert "centrality_score" in result["critical_hubs"][0]


# ============ Module 8: Shipment Clusterer ============

class TestShipmentClusterer:
    """Tests for Shipment Clusterer service."""

    @pytest.fixture
    def clusterer(self):
        return ShipmentClusterer()

    @pytest.fixture
    def tenant_id(self):
        return uuid4()

    @pytest.mark.asyncio
    async def test_cluster_shipments(self, clusterer, tenant_id):
        """Test shipment clustering."""
        result = await clusterer.cluster_shipments(
            tenant_id=tenant_id,
            cluster_count=3,
        )
        
        assert isinstance(result, list)
        assert len(result) <= 3
        
        if result:
            assert "centroid" in result[0]
            assert "shipments" in result[0]
            assert "recommended_hub" in result[0]

    @pytest.mark.asyncio
    async def test_route_optimization_savings(self, clusterer, tenant_id):
        """Test route optimization calculations."""
        result = await clusterer.cluster_shipments(
            tenant_id=tenant_id,
            cluster_count=5,
        )
        
        if result:
            assert "route_optimization" in result[0]
            assert "savings_km" in result[0]["route_optimization"]


# ============ Module 9: ETA Predictor ============

class TestETAPredictor:
    """Tests for ETA Predictor service."""

    @pytest.fixture
    def predictor(self):
        return ETAPredictor()

    @pytest.fixture
    def tenant_id(self):
        return uuid4()

    @pytest.mark.asyncio
    async def test_predict_eta(self, predictor, tenant_id):
        """Test ETA prediction."""
        result = await predictor.predict_eta(
            tenant_id=tenant_id,
            shipment_id="ship-001",
        )
        
        assert "predicted_eta" in result
        assert "confidence_score" in result
        assert "factors" in result

    @pytest.mark.asyncio
    async def test_eta_factors(self, predictor, tenant_id):
        """Test factor analysis."""
        result = await predictor.predict_eta(
            tenant_id=tenant_id,
            shipment_id="ship-002",
        )
        
        factors = result["factors"]
        assert "driver_performance" in factors
        assert "route_reliability" in factors
        assert "traffic_conditions" in factors


# ============ Module 10: Driver Performance ============

class TestDriverPerformanceAnalyzer:
    """Tests for Driver Performance Analyzer service."""

    @pytest.fixture
    def analyzer(self):
        return DriverPerformanceAnalyzer()

    @pytest.fixture
    def tenant_id(self):
        return uuid4()

    @pytest.mark.asyncio
    async def test_analyze_driver(self, analyzer, tenant_id):
        """Test driver analysis."""
        result = await analyzer.analyze_driver(
            tenant_id=tenant_id,
            driver_id="driver-001",
        )
        
        assert "metrics" in result
        assert "rating" in result
        assert "comparison" in result
        assert "recommendations" in result

    @pytest.mark.asyncio
    async def test_leaderboard(self, analyzer, tenant_id):
        """Test driver leaderboard."""
        result = await analyzer.get_leaderboard(
            tenant_id=tenant_id,
            limit=5,
        )
        
        assert isinstance(result, list)
        assert len(result) <= 5
        
        if result:
            assert "rank" in result[0]
            assert "on_time_rate" in result[0]
