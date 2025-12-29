"""
Analytics Engine Service - Dashboard KPIs

Generates operational dashboard metrics:
- Real-time KPIs
- Time series analysis
- Route analytics
- Fleet utilization
"""

from datetime import datetime, timedelta
from typing import Any
from uuid import UUID

import numpy as np
import pandas as pd

from parhelion_py.infrastructure.external.parhelion_client import ParhelionApiClient


class AnalyticsEngine:
    """
    Comprehensive analytics engine for operational dashboards.
    
    Calculates KPIs, trends, and generates visualizations
    for logistics operations.
    """

    def __init__(self):
        pass

    async def generate_dashboard(
        self,
        tenant_id: UUID,
        refresh_cache: bool = False,
    ) -> dict[str, Any]:
        """
        Generate complete operational dashboard.
        
        Args:
            tenant_id: Tenant identifier
            refresh_cache: Force refresh cached data
            
        Returns:
            Dict with KPIs, time series, and analytics
        """
        # Fetch data (simulated)
        shipments = self._generate_sample_shipments()
        trucks = self._generate_sample_trucks()
        drivers = self._generate_sample_drivers()
        
        df_shipments = pd.DataFrame(shipments)
        df_trucks = pd.DataFrame(trucks)
        df_drivers = pd.DataFrame(drivers)
        
        # Calculate KPIs
        kpis = self._calculate_kpis(df_shipments, df_trucks, df_drivers)
        
        # Generate time series
        time_series = self._generate_time_series(df_shipments)
        
        # Route analytics
        route_analytics = self._analyze_routes(df_shipments)
        
        # Congestion analysis
        congestion = self._analyze_congestion(df_shipments)
        
        return {
            "tenant_id": str(tenant_id),
            "generated_at": datetime.now().isoformat(),
            "cache_status": "fresh" if refresh_cache else "cached",
            "kpis": kpis,
            "time_series": time_series,
            "route_analytics": route_analytics,
            "congestion_hotspots": congestion,
        }

    def _calculate_kpis(
        self,
        shipments: pd.DataFrame,
        trucks: pd.DataFrame,
        drivers: pd.DataFrame,
    ) -> dict[str, Any]:
        """Calculate operational KPIs."""
        today = datetime.now().date()
        
        # Filter today's shipments
        shipments["date"] = pd.to_datetime(shipments["created_at"]).dt.date
        today_shipments = shipments[shipments["date"] == today]
        
        # Total shipments today
        total_today = len(today_shipments)
        
        # On-time delivery rate
        delivered = shipments[shipments["status"] == "Delivered"]
        if len(delivered) > 0:
            on_time = delivered[delivered["is_on_time"] == True]
            otd_rate = len(on_time) / len(delivered) * 100
        else:
            otd_rate = 0.0
        
        # Fleet utilization
        if len(trucks) > 0:
            trucks["utilization"] = trucks["current_load_kg"] / trucks["max_capacity_kg"]
            avg_utilization = trucks["utilization"].mean() * 100
        else:
            avg_utilization = 0.0
        
        # Active drivers
        active_drivers = len(drivers[drivers["status"] == "OnRoute"])
        idle_drivers = len(drivers[drivers["status"] == "Available"])
        
        # Shipments at risk (delayed)
        at_risk = len(shipments[shipments["status"] == "Exception"])
        
        # Average transit time
        completed = shipments[shipments["status"] == "Delivered"]
        if len(completed) > 0:
            avg_transit = completed["transit_hours"].mean()
        else:
            avg_transit = 0.0
        
        # Idle trucks
        idle_trucks = len(trucks[trucks["current_load_kg"] == 0])
        
        return {
            "total_shipments_today": total_today,
            "on_time_delivery_rate": round(otd_rate, 1),
            "avg_truck_utilization": round(avg_utilization, 1),
            "shipments_at_risk": at_risk,
            "avg_transit_time_hours": round(avg_transit, 2),
            "active_drivers": active_drivers,
            "idle_drivers": idle_drivers,
            "idle_trucks": idle_trucks,
            "in_transit_shipments": len(shipments[shipments["status"] == "InTransit"]),
        }

    def _generate_time_series(
        self, shipments: pd.DataFrame
    ) -> dict[str, list[dict]]:
        """Generate time series data."""
        # Daily shipments (last 30 days)
        shipments["date"] = pd.to_datetime(shipments["created_at"]).dt.date
        daily = shipments.groupby("date").size().reset_index(name="count")
        
        shipments_over_time = [
            {"date": str(row["date"]), "value": int(row["count"])}
            for _, row in daily.iterrows()
        ]
        
        # Status distribution
        status_dist = shipments["status"].value_counts().to_dict()
        
        return {
            "shipments_over_time": shipments_over_time[-30:],
            "status_distribution": [
                {"status": k, "count": int(v)}
                for k, v in status_dist.items()
            ],
        }

    def _analyze_routes(self, shipments: pd.DataFrame) -> dict[str, Any]:
        """Analyze route performance."""
        # Group by route (origin -> destination)
        route_stats = shipments.groupby(
            ["origin_id", "destination_id"]
        ).agg({
            "id": "count",
            "is_on_time": "sum",
            "transit_hours": "mean",
        }).reset_index()
        
        route_stats.columns = [
            "origin", "destination", "shipment_count", "on_time_count", "avg_transit"
        ]
        
        route_stats["delay_rate"] = 1 - (
            route_stats["on_time_count"] / route_stats["shipment_count"]
        )
        
        # Top routes
        top_routes = route_stats.nlargest(5, "shipment_count")
        
        # Problem routes (high delay)
        problem_routes = route_stats[route_stats["delay_rate"] > 0.2]
        
        return {
            "top_routes": [
                {
                    "origin": row["origin"],
                    "destination": row["destination"],
                    "shipment_count": int(row["shipment_count"]),
                    "avg_transit_hours": round(row["avg_transit"], 1),
                    "delay_rate": round(row["delay_rate"] * 100, 1),
                }
                for _, row in top_routes.iterrows()
            ],
            "problem_routes": [
                {
                    "origin": row["origin"],
                    "destination": row["destination"],
                    "delay_rate": round(row["delay_rate"] * 100, 1),
                }
                for _, row in problem_routes.iterrows()
            ],
        }

    def _analyze_congestion(self, shipments: pd.DataFrame) -> list[dict]:
        """Identify congestion hotspots."""
        # Locations with most waiting shipments
        waiting = shipments[shipments["status"].isin(["AtHub", "Pending"])]
        
        congestion = waiting.groupby("destination_id").size().reset_index(name="waiting_count")
        congestion = congestion.nlargest(5, "waiting_count")
        
        return [
            {
                "location_id": row["destination_id"],
                "waiting_shipments": int(row["waiting_count"]),
                "severity": "HIGH" if row["waiting_count"] > 20 else "MEDIUM",
            }
            for _, row in congestion.iterrows()
        ]

    def _generate_sample_shipments(self) -> list[dict]:
        """Generate sample shipment data."""
        np.random.seed(42)
        shipments = []
        
        statuses = ["Pending", "InTransit", "AtHub", "Delivered", "Exception"]
        origins = ["MTY-HUB", "CDMX-HUB", "GDL-HUB"]
        destinations = ["PUE-WH", "VER-HUB", "TOR-WH", "QRO-WH"]
        
        for i in range(200):
            days_ago = np.random.randint(0, 30)
            created = datetime.now() - timedelta(days=days_ago)
            status = np.random.choice(statuses, p=[0.1, 0.3, 0.1, 0.45, 0.05])
            
            shipments.append({
                "id": f"ship-{i:04d}",
                "created_at": created.isoformat(),
                "status": status,
                "origin_id": np.random.choice(origins),
                "destination_id": np.random.choice(destinations),
                "is_on_time": np.random.random() > 0.15,
                "transit_hours": np.random.uniform(4, 24),
            })
        
        return shipments

    def _generate_sample_trucks(self) -> list[dict]:
        """Generate sample truck data."""
        return [
            {"id": f"truck-{i}", "max_capacity_kg": 5000, "current_load_kg": np.random.randint(0, 5000)}
            for i in range(20)
        ]

    def _generate_sample_drivers(self) -> list[dict]:
        """Generate sample driver data."""
        statuses = ["Available", "OnRoute", "OffDuty"]
        return [
            {"id": f"driver-{i}", "status": np.random.choice(statuses)}
            for i in range(30)
        ]
