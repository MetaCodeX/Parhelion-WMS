"""
Analytics API Router.

Provides endpoints for shipment analytics, fleet metrics,
and historical data analysis.
"""

import logging
from datetime import datetime
from typing import Annotated
from uuid import UUID

from fastapi import APIRouter, Depends, Query
from pydantic import BaseModel, Field

from parhelion_py.api.middleware.auth import AuthContextDep
from parhelion_py.api.middleware.tenant import TenantContextDep
from parhelion_py.infrastructure.external.parhelion_client import ParhelionApiClient

router = APIRouter(prefix="/api/py/analytics", tags=["Analytics"])
logger = logging.getLogger(__name__)


# ============ DTOs ============

class ShipmentAnalyticsRequest(BaseModel):
    """Request parameters for shipment analytics."""
    status: str | None = Field(None, description="Filter by status")
    limit: int = Field(100, ge=1, le=1000, description="Max records")


class ShipmentMetrics(BaseModel):
    """Aggregated shipment metrics."""
    total_shipments: int
    by_status: dict[str, int]
    avg_items_per_shipment: float
    time_range: dict[str, str | None]


class ShipmentAnalyticsResponse(BaseModel):
    """Response for shipment analytics endpoint."""
    tenant_id: str
    generated_at: datetime
    metrics: ShipmentMetrics
    shipments: list[dict]


# ============ Endpoints ============

@router.get(
    "/shipments",
    response_model=ShipmentAnalyticsResponse,
    summary="Get shipment analytics",
    description="Analyze shipments for a tenant with optional status filter."
)
async def get_shipment_analytics(
    auth: AuthContextDep,
    tenant: TenantContextDep,
    status: Annotated[str | None, Query(description="Filter by shipment status")] = None,
    limit: Annotated[int, Query(ge=1, le=1000)] = 100,
) -> ShipmentAnalyticsResponse:
    """
    Fetch and analyze shipments for the authenticated tenant.
    
    Returns aggregated metrics and raw shipment data.
    """
    logger.info(
        "Shipment analytics requested",
        extra={
            "tenant_id": str(tenant.id),
            "auth_source": auth.source,
            "status_filter": status,
            "limit": limit,
        }
    )
    
    async with ParhelionApiClient() as client:
        shipments = await client.get_shipments(
            tenant_id=tenant.id,
            status=status,
            limit=limit,
        )
    
    # Calculate metrics
    status_counts: dict[str, int] = {}
    total_items = 0
    earliest: str | None = None
    latest: str | None = None
    
    for s in shipments:
        s_status = s.get("status", "Unknown")
        status_counts[s_status] = status_counts.get(s_status, 0) + 1
        total_items += s.get("items_count", 0)
        
        created = s.get("created_at")
        if created:
            if earliest is None or created < earliest:
                earliest = created
            if latest is None or created > latest:
                latest = created
    
    metrics = ShipmentMetrics(
        total_shipments=len(shipments),
        by_status=status_counts,
        avg_items_per_shipment=total_items / len(shipments) if shipments else 0.0,
        time_range={"earliest": earliest, "latest": latest},
    )
    
    logger.info(
        "Shipment analytics completed",
        extra={
            "tenant_id": str(tenant.id),
            "total_shipments": metrics.total_shipments,
        }
    )
    
    return ShipmentAnalyticsResponse(
        tenant_id=str(tenant.id),
        generated_at=datetime.utcnow(),
        metrics=metrics,
        shipments=shipments,
    )


@router.get(
    "/fleet",
    summary="Get fleet analytics",
    description="Analyze fleet utilization and driver metrics."
)
async def get_fleet_analytics(
    auth: AuthContextDep,
    tenant: TenantContextDep,
) -> dict:
    """
    Fetch and analyze fleet data for the authenticated tenant.
    
    Returns driver availability and utilization metrics.
    """
    logger.info(
        "Fleet analytics requested",
        extra={"tenant_id": str(tenant.id), "auth_source": auth.source}
    )
    
    async with ParhelionApiClient() as client:
        all_drivers = await client.get_drivers(tenant_id=tenant.id)
        available_drivers = await client.get_drivers(
            tenant_id=tenant.id,
            available_only=True,
        )
    
    total = len(all_drivers)
    available = len(available_drivers)
    
    metrics = {
        "total_drivers": total,
        "available_drivers": available,
        "utilization_rate": (total - available) / total if total > 0 else 0.0,
        "drivers_with_location": sum(
            1 for d in all_drivers
            if d.get("last_latitude") and d.get("last_longitude")
        ),
    }
    
    logger.info(
        "Fleet analytics completed",
        extra={
            "tenant_id": str(tenant.id),
            "total_drivers": total,
            "utilization_rate": metrics["utilization_rate"],
        }
    )
    
    return {
        "tenant_id": str(tenant.id),
        "generated_at": datetime.utcnow().isoformat(),
        "metrics": metrics,
        "drivers": all_drivers,
    }
