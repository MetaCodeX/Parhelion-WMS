"""
Internal ML Services Router.

Endpoints for internal use by .NET API only.
No authentication required - trusts Docker network isolation.

All endpoints receive tenant_id and other context as request body parameters,
NOT from headers or JWT claims.
"""

import logging
from datetime import datetime
from typing import Any
from uuid import UUID

from fastapi import APIRouter
from pydantic import BaseModel, Field

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

router = APIRouter(prefix="/internal", tags=["Internal ML Services"])
logger = logging.getLogger(__name__)


# ============ Request Models (include tenant_id) ============

class RouteOptimizeRequest(BaseModel):
    """Request for route optimization."""
    tenant_id: UUID
    origin_id: str
    destination_id: str
    avoid_locations: list[str] = Field(default=[])
    max_time_hours: float | None = None


class TruckRecommendRequest(BaseModel):
    """Request for truck recommendations."""
    tenant_id: UUID
    shipment_id: str
    limit: int = Field(3, ge=1, le=10)
    consider_deadhead: bool = True


class DemandForecastRequest(BaseModel):
    """Request for demand forecasting."""
    tenant_id: UUID
    location_id: str | None = None
    days: int = Field(30, ge=1, le=90)


class AnomalyDetectRequest(BaseModel):
    """Request for anomaly detection."""
    tenant_id: UUID
    hours_back: int = Field(24, ge=1, le=168)
    severity_filter: str | None = None


class LoadingOptimizeRequest(BaseModel):
    """Request for 3D loading optimization."""
    tenant_id: UUID
    truck_id: str
    shipment_ids: list[str]


class DashboardRequest(BaseModel):
    """Request for dashboard generation."""
    tenant_id: UUID
    refresh_cache: bool = False


class NetworkAnalyzeRequest(BaseModel):
    """Request for network analysis."""
    tenant_id: UUID


class ShipmentClusterRequest(BaseModel):
    """Request for shipment clustering."""
    tenant_id: UUID
    cluster_count: int = Field(5, ge=2, le=20)
    date_from: str | None = None
    date_to: str | None = None


class ETAPredictRequest(BaseModel):
    """Request for ETA prediction."""
    tenant_id: UUID
    shipment_id: str


class DriverPerformanceRequest(BaseModel):
    """Request for driver performance analysis."""
    tenant_id: UUID
    driver_id: str
    days_back: int = Field(30, ge=7, le=90)


class LeaderboardRequest(BaseModel):
    """Request for driver leaderboard."""
    tenant_id: UUID
    limit: int = Field(10, ge=5, le=50)
    days_back: int = Field(30, ge=7, le=90)


# ============ Endpoints ============

@router.post("/routes/optimize", summary="[INTERNAL] Optimize route")
async def optimize_route(request: RouteOptimizeRequest) -> dict[str, Any]:
    """Calculate optimal route using graph algorithms."""
    logger.info(f"[Internal] Route optimize: {request.origin_id} -> {request.destination_id}")
    
    optimizer = RouteOptimizer()
    result = await optimizer.calculate_optimal_route(
        tenant_id=request.tenant_id,
        origin_id=request.origin_id,
        destination_id=request.destination_id,
        constraints={
            "avoid_locations": request.avoid_locations,
            "max_time": request.max_time_hours,
        },
    )
    return result


@router.post("/trucks/recommend", summary="[INTERNAL] Recommend trucks")
async def recommend_trucks(request: TruckRecommendRequest) -> list[dict[str, Any]]:
    """Recommend optimal trucks for a shipment."""
    logger.info(f"[Internal] Truck recommend for: {request.shipment_id}")
    
    recommender = TruckRecommender()
    result = await recommender.recommend_trucks(
        tenant_id=request.tenant_id,
        shipment_id=request.shipment_id,
        limit=request.limit,
        consider_deadhead=request.consider_deadhead,
    )
    return result


@router.post("/forecast/demand", summary="[INTERNAL] Forecast demand")
async def forecast_demand(request: DemandForecastRequest) -> dict[str, Any]:
    """Predict future shipment demand."""
    logger.info(f"[Internal] Demand forecast for {request.days} days")
    
    forecaster = DemandForecaster()
    result = await forecaster.forecast_demand(
        tenant_id=request.tenant_id,
        location_id=request.location_id,
        days=request.days,
    )
    return result


@router.post("/anomalies/detect", summary="[INTERNAL] Detect anomalies")
async def detect_anomalies(request: AnomalyDetectRequest) -> list[dict[str, Any]]:
    """Detect anomalies in shipment tracking."""
    logger.info(f"[Internal] Anomaly detection for last {request.hours_back} hours")
    
    detector = AnomalyDetector()
    result = await detector.detect_anomalies(
        tenant_id=request.tenant_id,
        hours_back=request.hours_back,
        severity_filter=request.severity_filter,
    )
    return result


@router.post("/loading/optimize", summary="[INTERNAL] Optimize 3D loading")
async def optimize_loading(request: LoadingOptimizeRequest) -> dict[str, Any]:
    """Calculate optimal 3D cargo arrangement."""
    logger.info(f"[Internal] Loading optimize for truck: {request.truck_id}")
    
    optimizer = LoadingOptimizer()
    result = await optimizer.optimize_loading(
        tenant_id=request.tenant_id,
        truck_id=request.truck_id,
        shipment_ids=request.shipment_ids,
    )
    return result


@router.post("/dashboard/generate", summary="[INTERNAL] Generate dashboard")
async def generate_dashboard(request: DashboardRequest) -> dict[str, Any]:
    """Generate operational dashboard with KPIs."""
    logger.info("[Internal] Dashboard generation requested")
    
    engine = AnalyticsEngine()
    result = await engine.generate_dashboard(
        tenant_id=request.tenant_id,
        refresh_cache=request.refresh_cache,
    )
    return result


@router.post("/network/analyze", summary="[INTERNAL] Analyze network")
async def analyze_network(request: NetworkAnalyzeRequest) -> dict[str, Any]:
    """Analyze hub & spoke network topology."""
    logger.info("[Internal] Network analysis requested")
    
    analyzer = NetworkAnalyzer()
    result = await analyzer.analyze_network(tenant_id=request.tenant_id)
    return result


@router.post("/shipments/cluster", summary="[INTERNAL] Cluster shipments")
async def cluster_shipments(request: ShipmentClusterRequest) -> list[dict[str, Any]]:
    """Group shipments geographically for route consolidation."""
    logger.info(f"[Internal] Clustering into {request.cluster_count} groups")
    
    clusterer = ShipmentClusterer()
    result = await clusterer.cluster_shipments(
        tenant_id=request.tenant_id,
        cluster_count=request.cluster_count,
        date_from=request.date_from,
        date_to=request.date_to,
    )
    return result


@router.post("/eta/predict", summary="[INTERNAL] Predict ETA")
async def predict_eta(request: ETAPredictRequest) -> dict[str, Any]:
    """Predict dynamic ETA using ML."""
    logger.info(f"[Internal] ETA prediction for: {request.shipment_id}")
    
    predictor = ETAPredictor()
    result = await predictor.predict_eta(
        tenant_id=request.tenant_id,
        shipment_id=request.shipment_id,
    )
    return result


@router.post("/drivers/performance", summary="[INTERNAL] Driver performance")
async def analyze_driver_performance(request: DriverPerformanceRequest) -> dict[str, Any]:
    """Get driver performance metrics."""
    logger.info(f"[Internal] Performance for driver: {request.driver_id}")
    
    analyzer = DriverPerformanceAnalyzer()
    result = await analyzer.analyze_driver(
        tenant_id=request.tenant_id,
        driver_id=request.driver_id,
        days_back=request.days_back,
    )
    return result


@router.post("/drivers/leaderboard", summary="[INTERNAL] Driver leaderboard")
async def get_leaderboard(request: LeaderboardRequest) -> list[dict[str, Any]]:
    """Get top performing drivers."""
    logger.info(f"[Internal] Driver leaderboard, top {request.limit}")
    
    analyzer = DriverPerformanceAnalyzer()
    result = await analyzer.get_leaderboard(
        tenant_id=request.tenant_id,
        days_back=request.days_back,
        limit=request.limit,
    )
    return result
