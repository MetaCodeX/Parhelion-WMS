"""
Shipment Clusterer Service - K-Means Clustering

Clusters shipments geographically for:
- Route consolidation
- Zone-based delivery
- Optimal hub assignment
"""

from datetime import datetime
from typing import Any
from uuid import UUID

import numpy as np
import pandas as pd
from sklearn.cluster import KMeans
from sklearn.preprocessing import StandardScaler


class ShipmentClusterer:
    """
    Geographic clustering for shipment optimization.
    
    Uses K-Means to group shipments by destination,
    enabling consolidated deliveries.
    """

    def __init__(self):
        self._scaler = StandardScaler()

    async def cluster_shipments(
        self,
        tenant_id: UUID,
        cluster_count: int = 5,
        date_from: str | None = None,
        date_to: str | None = None,
    ) -> list[dict[str, Any]]:
        """
        Cluster pending shipments by destination.
        
        Args:
            tenant_id: Tenant identifier
            cluster_count: Number of clusters to create
            date_from: Start date filter (ISO format)
            date_to: End date filter (ISO format)
            
        Returns:
            List of clusters with shipments and recommendations
        """
        # Get shipments (simulated)
        shipments = self._generate_sample_shipments()
        
        if not shipments:
            return []
        
        df = pd.DataFrame(shipments)
        
        # Extract features for clustering
        features = df[["dest_lat", "dest_lon", "weight_kg", "volume_m3"]].copy()
        
        # Normalize
        features_scaled = self._scaler.fit_transform(features)
        
        # K-Means clustering
        kmeans = KMeans(
            n_clusters=min(cluster_count, len(df)),
            random_state=42,
            n_init=10,
        )
        
        df["cluster_id"] = kmeans.fit_predict(features_scaled)
        
        # Analyze each cluster
        clusters = []
        
        for cluster_id in sorted(df["cluster_id"].unique()):
            cluster_df = df[df["cluster_id"] == cluster_id]
            
            # Calculate centroid
            centroid_lat = cluster_df["dest_lat"].mean()
            centroid_lon = cluster_df["dest_lon"].mean()
            
            # Find nearest hub
            nearest_hub = self._find_nearest_hub(centroid_lat, centroid_lon)
            
            # Calculate totals
            total_weight = cluster_df["weight_kg"].sum()
            total_volume = cluster_df["volume_m3"].sum()
            
            # Recommend truck type
            truck_type = self._recommend_truck(total_weight, total_volume)
            
            # Calculate estimated savings
            individual_km = cluster_df["distance_to_hub_km"].sum()
            clustered_km = self._calculate_route_distance(cluster_df)
            savings_km = individual_km - clustered_km
            
            clusters.append({
                "cluster_id": int(cluster_id),
                "centroid": {
                    "latitude": round(centroid_lat, 6),
                    "longitude": round(centroid_lon, 6),
                },
                "shipments": [
                    {
                        "id": row["id"],
                        "tracking_number": row["tracking_number"],
                        "recipient_name": row["recipient_name"],
                        "weight_kg": row["weight_kg"],
                        "destination": f"{row['dest_lat']:.4f}, {row['dest_lon']:.4f}",
                    }
                    for _, row in cluster_df.iterrows()
                ],
                "shipment_count": len(cluster_df),
                "total_weight_kg": round(total_weight, 2),
                "total_volume_m3": round(total_volume, 3),
                "recommended_hub": nearest_hub,
                "recommended_truck_type": truck_type,
                "route_optimization": {
                    "individual_km": round(individual_km, 1),
                    "optimized_km": round(clustered_km, 1),
                    "savings_km": round(savings_km, 1),
                    "savings_pct": round(savings_km / individual_km * 100, 1) if individual_km > 0 else 0,
                },
            })
        
        return clusters

    def _generate_sample_shipments(self) -> list[dict]:
        """Generate sample shipment data."""
        np.random.seed(42)
        shipments = []
        
        # Simulated destinations around different zones
        zones = [
            (25.68, -100.32, "MTY"),  # Monterrey
            (20.67, -103.35, "GDL"),  # Guadalajara
            (19.43, -99.13, "CDMX"),  # Mexico City
        ]
        
        for i in range(30):
            zone = zones[i % 3]
            base_lat, base_lon, zone_code = zone
            
            # Add some variance within zone
            lat = base_lat + np.random.uniform(-0.1, 0.1)
            lon = base_lon + np.random.uniform(-0.1, 0.1)
            
            shipments.append({
                "id": f"ship-{i:04d}",
                "tracking_number": f"TRX-2025-{i:04d}",
                "recipient_name": f"Customer {i+1}",
                "dest_lat": lat,
                "dest_lon": lon,
                "zone": zone_code,
                "weight_kg": np.random.uniform(10, 100),
                "volume_m3": np.random.uniform(0.1, 1.0),
                "distance_to_hub_km": np.random.uniform(10, 50),
            })
        
        return shipments

    def _find_nearest_hub(self, lat: float, lon: float) -> dict:
        """Find nearest hub to given coordinates."""
        hubs = [
            {"code": "MTY", "name": "Monterrey Hub", "lat": 25.68, "lon": -100.32},
            {"code": "GDL", "name": "Guadalajara Hub", "lat": 20.67, "lon": -103.35},
            {"code": "CDMX", "name": "Mexico City Hub", "lat": 19.43, "lon": -99.13},
        ]
        
        min_dist = float("inf")
        nearest = hubs[0]
        
        for hub in hubs:
            dist = ((hub["lat"] - lat) ** 2 + (hub["lon"] - lon) ** 2) ** 0.5
            if dist < min_dist:
                min_dist = dist
                nearest = hub
        
        return {
            "code": nearest["code"],
            "name": nearest["name"],
            "distance_km": round(min_dist * 111, 1),  # Approximate km
        }

    def _recommend_truck(self, weight_kg: float, volume_m3: float) -> str:
        """Recommend truck type based on load."""
        if weight_kg < 500 and volume_m3 < 5:
            return "Van"
        elif weight_kg < 2000 and volume_m3 < 20:
            return "Light Truck"
        elif weight_kg < 5000 and volume_m3 < 40:
            return "Medium Truck"
        else:
            return "Heavy Truck"

    def _calculate_route_distance(self, cluster_df: pd.DataFrame) -> float:
        """Calculate optimized route distance for cluster."""
        if len(cluster_df) <= 1:
            return cluster_df["distance_to_hub_km"].sum()
        
        # Simple estimate: hub to centroid + local delivery
        centroid_lat = cluster_df["dest_lat"].mean()
        centroid_lon = cluster_df["dest_lon"].mean()
        
        # Distance from hub to centroid
        hub_to_centroid = 20  # Approximate
        
        # Local delivery within cluster (assume ~5km between stops)
        local_delivery = (len(cluster_df) - 1) * 5
        
        return hub_to_centroid + local_delivery
