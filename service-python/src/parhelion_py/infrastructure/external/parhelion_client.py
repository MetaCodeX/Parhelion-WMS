"""
Anti-Corruption Layer: HTTP Client for .NET API communication.

This module provides a clean interface to communicate with the main
.NET Parhelion API, translating between Python and .NET domain models.
"""

from typing import Any
from uuid import UUID

import httpx

from parhelion_py.infrastructure.config.settings import get_settings


class ParhelionApiClient:
    """
    Anti-Corruption Layer (ACL) for .NET API communication.
    
    Translates .NET API responses into Python domain models,
    ensuring the Python service is not coupled to .NET DTOs.
    """

    def __init__(self, http_client: httpx.AsyncClient | None = None):
        self._settings = get_settings()
        self._base_url = str(self._settings.parhelion_api_url).rstrip("/")
        self._http = http_client or httpx.AsyncClient(timeout=30.0)

    def _auth_headers(self, tenant_id: UUID | None = None) -> dict[str, str]:
        """Build authentication headers for inter-service calls."""
        headers = {
            "X-Internal-Service-Key": self._settings.internal_service_key,
            "Content-Type": "application/json",
        }
        if tenant_id:
            headers["X-Tenant-Id"] = str(tenant_id)
        return headers

    async def health_check(self) -> bool:
        """Check if .NET API is healthy."""
        try:
            response = await self._http.get(
                f"{self._base_url}/health",
                timeout=5.0
            )
            return response.status_code == 200
        except httpx.RequestError:
            return False

    async def get_shipments(
        self,
        tenant_id: UUID,
        status: str | None = None,
        limit: int = 100,
    ) -> list[dict[str, Any]]:
        """
        Fetch shipments from .NET API.
        
        Args:
            tenant_id: Tenant identifier for multi-tenant filtering
            status: Optional status filter (e.g., "InTransit", "Delivered")
            limit: Maximum number of records to fetch
            
        Returns:
            List of shipment dictionaries (transformed from .NET DTOs)
        """
        params: dict[str, Any] = {"pageSize": limit}
        if status:
            params["status"] = status

        response = await self._http.get(
            f"{self._base_url}/api/shipments",
            params=params,
            headers=self._auth_headers(tenant_id),
        )
        response.raise_for_status()
        
        data = response.json()
        # Transform .NET DTO to Python-friendly format
        return self._transform_shipments(data.get("items", []))

    async def get_drivers(
        self,
        tenant_id: UUID,
        available_only: bool = False,
    ) -> list[dict[str, Any]]:
        """
        Fetch drivers from .NET API.
        
        Args:
            tenant_id: Tenant identifier
            available_only: If True, only return available drivers
            
        Returns:
            List of driver dictionaries
        """
        params: dict[str, Any] = {}
        if available_only:
            params["available"] = "true"

        response = await self._http.get(
            f"{self._base_url}/api/drivers",
            params=params,
            headers=self._auth_headers(tenant_id),
        )
        response.raise_for_status()
        
        data = response.json()
        return self._transform_drivers(data.get("items", []))

    async def get_checkpoints(
        self,
        tenant_id: UUID,
        shipment_id: UUID,
    ) -> list[dict[str, Any]]:
        """
        Fetch checkpoints for a specific shipment.
        
        Args:
            tenant_id: Tenant identifier
            shipment_id: Shipment to get checkpoints for
            
        Returns:
            List of checkpoint dictionaries ordered by timestamp
        """
        response = await self._http.get(
            f"{self._base_url}/api/shipment-checkpoints/timeline/{shipment_id}",
            headers=self._auth_headers(tenant_id),
        )
        response.raise_for_status()
        
        return response.json()

    async def get_nearby_drivers(
        self,
        tenant_id: UUID,
        latitude: float,
        longitude: float,
        radius_km: float = 50.0,
    ) -> list[dict[str, Any]]:
        """
        Find drivers near a specific location (Haversine).
        
        Args:
            tenant_id: Tenant identifier
            latitude: Center point latitude
            longitude: Center point longitude
            radius_km: Search radius in kilometers
            
        Returns:
            List of nearby drivers with distance
        """
        response = await self._http.get(
            f"{self._base_url}/api/drivers/nearby",
            params={
                "latitude": latitude,
                "longitude": longitude,
                "radiusKm": radius_km,
            },
            headers=self._auth_headers(tenant_id),
        )
        response.raise_for_status()
        
        return response.json()

    def _transform_shipments(self, items: list[dict]) -> list[dict[str, Any]]:
        """Transform .NET shipment DTOs to Python format."""
        return [
            {
                "id": item.get("id"),
                "tracking_number": item.get("trackingNumber"),
                "status": item.get("status"),
                "origin_location_id": item.get("originLocationId"),
                "destination_location_id": item.get("destinationLocationId"),
                "driver_id": item.get("driverId"),
                "truck_id": item.get("truckId"),
                "created_at": item.get("createdAt"),
                "updated_at": item.get("updatedAt"),
                "items_count": len(item.get("items", [])),
            }
            for item in items
        ]

    def _transform_drivers(self, items: list[dict]) -> list[dict[str, Any]]:
        """Transform .NET driver DTOs to Python format."""
        return [
            {
                "id": item.get("id"),
                "name": f"{item.get('firstName', '')} {item.get('lastName', '')}".strip(),
                "license_number": item.get("licenseNumber"),
                "current_truck_id": item.get("currentTruckId"),
                "is_active": item.get("isActive", True),
                "last_latitude": item.get("lastLatitude"),
                "last_longitude": item.get("lastLongitude"),
            }
            for item in items
        ]

    async def close(self) -> None:
        """Close the HTTP client connection."""
        await self._http.aclose()

    async def __aenter__(self) -> "ParhelionApiClient":
        return self

    async def __aexit__(self, *args: Any) -> None:
        await self.close()
