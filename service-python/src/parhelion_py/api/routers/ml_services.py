"""
ML Services API Router

Exposes all 10 ML analytics modules via REST endpoints:
1. Route Optimization
2. Truck Recommendations
3. Demand Forecasting
4. Anomaly Detection
5. Loading Optimization
6. Dashboard Analytics
7. Network Analysis
8. Shipment Clustering
9. ETA Prediction
10. Driver Performance
"""

import logging
from typing import Any
from uuid import UUID

from fastapi import APIRouter, Query, HTTPException
from pydantic import BaseModel, Field

from parhelion_py.api.middleware.auth import AuthContextDep
from parhelion_py.api.middleware.tenant import TenantContextDep
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

router = APIRouter(prefix="/api/py", tags=["ML Analytics"])
logger = logging.getLogger(__name__)


# ============ Request/Response Models ============

class RouteRequest(BaseModel):
    origin_id: str = Field(..., description="Origin location ID")
    destination_id: str = Field(..., description="Destination location ID")
    avoid_locations: list[str] = Field(default=[], description="Locations to avoid")
    max_time_hours: float | None = Field(None, description="Max transit time")


class LoadingRequest(BaseModel):
    truck_id: str = Field(..., description="Truck ID to load")
    shipment_ids: list[str] = Field(..., description="Shipments to load")


class ClusterRequest(BaseModel):
    cluster_count: int = Field(5, ge=2, le=20, description="Number of clusters")
    date_from: str | None = None
    date_to: str | None = None


# ============ 1. Route Optimization ============

@router.post("/routes/optimize", summary="Optimize route between locations")
async def optimize_route(
    request: RouteRequest,
    auth: AuthContextDep,
    tenant: TenantContextDep,
) -> dict[str, Any]:
    """Calculate optimal route using graph algorithms."""
    logger.info(f"Route optimization: {request.origin_id} -> {request.destination_id}")
    
    optimizer = RouteOptimizer()
    result = await optimizer.calculate_optimal_route(
        tenant_id=tenant.id,
        origin_id=request.origin_id,
        destination_id=request.destination_id,
        constraints={
            "avoid_locations": request.avoid_locations,
            "max_time": request.max_time_hours,
        },
    )
    return result


# ============ 2. Truck Recommendations ============

@router.get("/trucks/recommend/{shipment_id}", summary="Get truck recommendations")
async def recommend_trucks(
    shipment_id: str,
    auth: AuthContextDep,
    tenant: TenantContextDep,
    limit: int = Query(3, ge=1, le=10),
    consider_deadhead: bool = Query(True),
) -> list[dict[str, Any]]:
    """Recommend optimal trucks for a shipment."""
    logger.info(f"Truck recommendation for shipment: {shipment_id}")
    
    recommender = TruckRecommender()
    result = await recommender.recommend_trucks(
        tenant_id=tenant.id,
        shipment_id=shipment_id,
        limit=limit,
        consider_deadhead=consider_deadhead,
    )
    return result


# ============ 3. Demand Forecasting ============

@router.get("/forecast/demand", summary="Forecast shipment demand")
async def forecast_demand(
    auth: AuthContextDep,
    tenant: TenantContextDep,
    location_id: str | None = Query(None),
    days: int = Query(30, ge=1, le=90),
) -> dict[str, Any]:
    """Predict future shipment demand."""
    logger.info(f"Demand forecast for {days} days")
    
    forecaster = DemandForecaster()
    result = await forecaster.forecast_demand(
        tenant_id=tenant.id,
        location_id=location_id,
        days=days,
    )
    return result


# ============ 4. Anomaly Detection ============

@router.get("/anomalies/detect", summary="Detect tracking anomalies")
async def detect_anomalies(
    auth: AuthContextDep,
    tenant: TenantContextDep,
    hours_back: int = Query(24, ge=1, le=168),
    severity: str | None = Query(None, pattern="^(CRITICAL|WARNING|INFO)$"),
) -> list[dict[str, Any]]:
    """Detect anomalies in shipment tracking."""
    logger.info(f"Anomaly detection for last {hours_back} hours")
    
    detector = AnomalyDetector()
    result = await detector.detect_anomalies(
        tenant_id=tenant.id,
        hours_back=hours_back,
        severity_filter=severity,
    )
    return result


# ============ 5. Loading Optimization ============

@router.post("/loading/optimize", summary="Optimize 3D cargo loading")
async def optimize_loading(
    request: LoadingRequest,
    auth: AuthContextDep,
    tenant: TenantContextDep,
) -> dict[str, Any]:
    """Calculate optimal 3D cargo arrangement."""
    logger.info(f"Loading optimization for truck: {request.truck_id}")
    
    optimizer = LoadingOptimizer()
    result = await optimizer.optimize_loading(
        tenant_id=tenant.id,
        truck_id=request.truck_id,
        shipment_ids=request.shipment_ids,
    )
    return result


# ============ 6. Dashboard Analytics ============

@router.get("/dashboard", summary="Get operational dashboard")
async def get_dashboard(
    auth: AuthContextDep,
    tenant: TenantContextDep,
    refresh: bool = Query(False),
) -> dict[str, Any]:
    """Generate complete operational dashboard with KPIs."""
    logger.info("Dashboard generation requested")
    
    engine = AnalyticsEngine()
    result = await engine.generate_dashboard(
        tenant_id=tenant.id,
        refresh_cache=refresh,
    )
    return result


# ============ 7. Network Analysis ============

@router.get("/network/analyze", summary="Analyze logistics network")
async def analyze_network(
    auth: AuthContextDep,
    tenant: TenantContextDep,
) -> dict[str, Any]:
    """Analyze hub & spoke network topology."""
    logger.info("Network analysis requested")
    
    analyzer = NetworkAnalyzer()
    result = await analyzer.analyze_network(tenant_id=tenant.id)
    return result


# ============ 8. Shipment Clustering ============

@router.post("/shipments/cluster", summary="Cluster shipments by zone")
async def cluster_shipments(
    request: ClusterRequest,
    auth: AuthContextDep,
    tenant: TenantContextDep,
) -> list[dict[str, Any]]:
    """Group shipments geographically for route consolidation."""
    logger.info(f"Clustering shipments into {request.cluster_count} groups")
    
    clusterer = ShipmentClusterer()
    result = await clusterer.cluster_shipments(
        tenant_id=tenant.id,
        cluster_count=request.cluster_count,
        date_from=request.date_from,
        date_to=request.date_to,
    )
    return result


# ============ 9. ETA Prediction ============

@router.get("/predictions/eta/{shipment_id}", summary="Predict shipment ETA")
async def predict_eta(
    shipment_id: str,
    auth: AuthContextDep,
    tenant: TenantContextDep,
) -> dict[str, Any]:
    """Predict dynamic ETA using ML."""
    logger.info(f"ETA prediction for shipment: {shipment_id}")
    
    predictor = ETAPredictor()
    result = await predictor.predict_eta(
        tenant_id=tenant.id,
        shipment_id=shipment_id,
    )
    return result


# ============ 10. Driver Performance ============

@router.get("/drivers/performance/{driver_id}", summary="Analyze driver performance")
async def analyze_driver(
    driver_id: str,
    auth: AuthContextDep,
    tenant: TenantContextDep,
    days: int = Query(30, ge=7, le=90),
) -> dict[str, Any]:
    """Get driver performance metrics and ranking."""
    logger.info(f"Performance analysis for driver: {driver_id}")
    
    analyzer = DriverPerformanceAnalyzer()
    result = await analyzer.analyze_driver(
        tenant_id=tenant.id,
        driver_id=driver_id,
        days_back=days,
    )
    return result


@router.get("/drivers/leaderboard", summary="Get driver leaderboard")
async def get_leaderboard(
    auth: AuthContextDep,
    tenant: TenantContextDep,
    limit: int = Query(10, ge=5, le=50),
    days: int = Query(30, ge=7, le=90),
) -> list[dict[str, Any]]:
    """Get top performing drivers."""
    logger.info(f"Driver leaderboard requested, top {limit}")
    
    analyzer = DriverPerformanceAnalyzer()
    result = await analyzer.get_leaderboard(
        tenant_id=tenant.id,
        days_back=days,
        limit=limit,
    )
    return result
