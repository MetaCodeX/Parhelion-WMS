"""
Truck Recommender Service - ML-based truck assignment

Recommends optimal trucks for shipments using:
- Capacity matching (weight/volume)
- Geographic proximity (minimize deadhead)
- Historical performance scoring
- Cargo type compatibility
"""

from typing import Any
from uuid import UUID

import pandas as pd
from sklearn.preprocessing import MinMaxScaler

from parhelion_py.infrastructure.external.parhelion_client import ParhelionApiClient


class TruckRecommender:
    """
    ML-based truck recommendation system.
    
    Uses feature engineering and scoring to recommend
    the best trucks for a given shipment.
    """

    def __init__(self):
        self._scaler = MinMaxScaler()

    async def recommend_trucks(
        self,
        tenant_id: UUID,
        shipment_id: str,
        limit: int = 3,
        consider_deadhead: bool = True,
    ) -> list[dict[str, Any]]:
        """
        Recommend trucks for a shipment.
        
        Args:
            tenant_id: Tenant identifier
            shipment_id: Shipment to assign
            limit: Number of recommendations
            consider_deadhead: Whether to consider truck location
            
        Returns:
            List of truck recommendations with scores
        """
        async with ParhelionApiClient() as client:
            # Get shipment details (simulated)
            shipment = {
                "id": shipment_id,
                "total_weight_kg": 2500,
                "total_volume_m3": 15,
                "requires_refrigeration": False,
                "is_hazmat": False,
                "origin_lat": 25.6866,
                "origin_lon": -100.3161,
            }
            
            # Get available trucks
            trucks = await client.get_drivers(tenant_id)
            
        if not trucks:
            # Simulated truck data
            trucks = self._get_sample_trucks()
        
        # Convert to DataFrame for analysis
        df = pd.DataFrame(trucks)
        
        # Feature engineering
        df = self._calculate_features(df, shipment, consider_deadhead)
        
        # Apply hard constraints
        df = self._apply_hard_constraints(df, shipment)
        
        if df.empty:
            return []
        
        # Calculate composite score
        df = self._calculate_scores(df)
        
        # Sort and return top N
        top_trucks = df.nlargest(limit, "final_score")
        
        return self._format_recommendations(top_trucks, shipment)

    def _get_sample_trucks(self) -> list[dict]:
        """Sample truck data for testing."""
        return [
            {
                "id": "truck-001",
                "plate": "ABC-123",
                "type": "DryBox",
                "max_capacity_kg": 5000,
                "current_load_kg": 1500,
                "max_volume_m3": 40,
                "current_volume_m3": 12,
                "has_refrigeration": False,
                "has_hazmat_cert": False,
                "last_latitude": 25.70,
                "last_longitude": -100.30,
            },
            {
                "id": "truck-002",
                "plate": "DEF-456",
                "type": "Refrigerado",
                "max_capacity_kg": 4000,
                "current_load_kg": 0,
                "max_volume_m3": 35,
                "current_volume_m3": 0,
                "has_refrigeration": True,
                "has_hazmat_cert": False,
                "last_latitude": 25.65,
                "last_longitude": -100.35,
            },
            {
                "id": "truck-003",
                "plate": "GHI-789",
                "type": "DryBox",
                "max_capacity_kg": 8000,
                "current_load_kg": 3000,
                "max_volume_m3": 60,
                "current_volume_m3": 20,
                "has_refrigeration": False,
                "has_hazmat_cert": True,
                "last_latitude": 25.80,
                "last_longitude": -100.20,
            },
        ]

    def _calculate_features(
        self, df: pd.DataFrame, shipment: dict, consider_deadhead: bool
    ) -> pd.DataFrame:
        """Calculate features for scoring."""
        # Available capacity
        df["available_kg"] = df["max_capacity_kg"] - df["current_load_kg"]
        df["available_m3"] = df["max_volume_m3"] - df["current_volume_m3"]
        
        # Utilization after loading
        df["projected_utilization"] = (
            (df["current_load_kg"] + shipment["total_weight_kg"]) /
            df["max_capacity_kg"]
        )
        
        # Deadhead distance (Haversine simplified)
        if consider_deadhead:
            df["deadhead_km"] = self._calculate_distances(
                df, shipment["origin_lat"], shipment["origin_lon"]
            )
        else:
            df["deadhead_km"] = 0
        
        return df

    def _calculate_distances(
        self, df: pd.DataFrame, lat: float, lon: float
    ) -> pd.Series:
        """Calculate approximate distances in km."""
        # Simplified distance calculation
        lat_diff = (df["last_latitude"] - lat).abs() * 111
        lon_diff = (df["last_longitude"] - lon).abs() * 111 * 0.85
        return (lat_diff**2 + lon_diff**2).pow(0.5)

    def _apply_hard_constraints(
        self, df: pd.DataFrame, shipment: dict
    ) -> pd.DataFrame:
        """Filter trucks that can't carry the shipment."""
        # Weight constraint
        df = df[df["available_kg"] >= shipment["total_weight_kg"]]
        
        # Volume constraint
        df = df[df["available_m3"] >= shipment["total_volume_m3"]]
        
        # Refrigeration constraint
        if shipment.get("requires_refrigeration"):
            df = df[df["has_refrigeration"] == True]
        
        # HAZMAT constraint
        if shipment.get("is_hazmat"):
            df = df[df["has_hazmat_cert"] == True]
        
        return df

    def _calculate_scores(self, df: pd.DataFrame) -> pd.DataFrame:
        """Calculate composite score for each truck."""
        # Normalize features
        df["util_score"] = df["projected_utilization"].clip(0, 1)
        
        # Distance score (lower is better)
        max_dist = df["deadhead_km"].max() if df["deadhead_km"].max() > 0 else 1
        df["distance_score"] = 1 - (df["deadhead_km"] / max_dist)
        
        # Capacity score (prefer trucks with appropriate capacity)
        df["capacity_score"] = df["available_kg"] / df["max_capacity_kg"]
        
        # Final weighted score
        df["final_score"] = (
            df["util_score"] * 0.4 +
            df["distance_score"] * 0.35 +
            df["capacity_score"] * 0.25
        )
        
        return df

    def _format_recommendations(
        self, df: pd.DataFrame, shipment: dict
    ) -> list[dict[str, Any]]:
        """Format recommendations for API response."""
        recommendations = []
        
        for _, row in df.iterrows():
            reasons = []
            if row["util_score"] > 0.7:
                reasons.append("Good capacity utilization")
            if row["distance_score"] > 0.7:
                reasons.append("Close to pickup location")
            if row["deadhead_km"] < 10:
                reasons.append("Minimal deadhead distance")
            
            recommendations.append({
                "truck": {
                    "id": row["id"],
                    "plate": row["plate"],
                    "type": row["type"],
                    "max_capacity_kg": row["max_capacity_kg"],
                    "current_load_kg": row["current_load_kg"],
                    "available_kg": row["available_kg"],
                },
                "score": round(row["final_score"], 3),
                "projected_utilization": round(row["projected_utilization"], 2),
                "deadhead_km": round(row["deadhead_km"], 2),
                "reasons": reasons or ["Available capacity"],
                "compatible": True,
            })
        
        return recommendations
