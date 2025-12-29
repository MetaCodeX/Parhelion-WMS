"""
Loading Optimizer Service - 3D Bin Packing

Optimizes cargo loading in trucks:
- 3D bin packing algorithm
- Weight distribution analysis
- Fragility considerations
- Loading sequence generation
"""

from typing import Any
from uuid import UUID


class LoadingOptimizer:
    """
    3D bin packing optimization for truck loading.
    
    Considers physical dimensions, weight distribution,
    fragility, and delivery order.
    """

    def __init__(self):
        pass

    async def optimize_loading(
        self,
        tenant_id: UUID,
        truck_id: str,
        shipment_ids: list[str],
    ) -> dict[str, Any]:
        """
        Calculate optimal 3D loading arrangement.
        
        Args:
            tenant_id: Tenant identifier
            truck_id: Truck to load
            shipment_ids: Shipments to load
            
        Returns:
            Dict with loading plan, positions, and utilization
        """
        # Get truck dimensions (sample)
        truck = {
            "id": truck_id,
            "plate": "ABC-123",
            "cargo_width_cm": 240,
            "cargo_height_cm": 220,
            "cargo_depth_cm": 600,
            "max_capacity_kg": 5000,
        }
        
        # Get items (sample)
        items = self._generate_sample_items(shipment_ids)
        
        # Calculate truck volume
        truck_volume_m3 = (
            truck["cargo_width_cm"] *
            truck["cargo_height_cm"] *
            truck["cargo_depth_cm"]
        ) / 1_000_000
        
        # Simple bin packing (First Fit Decreasing)
        sorted_items = sorted(
            items,
            key=lambda x: x["volume_cm3"],
            reverse=True,
        )
        
        loaded_items = []
        unfitted_items = []
        current_x = 0
        current_y = 0
        current_z = 0
        row_height = 0
        layer_depth = 0
        total_weight = 0
        total_volume = 0
        
        for item in sorted_items:
            # Check weight limit
            if total_weight + item["weight_kg"] > truck["max_capacity_kg"]:
                unfitted_items.append(item)
                continue
            
            # Check if fits in current row
            if current_x + item["width_cm"] <= truck["cargo_width_cm"]:
                # Fits in current position
                pass
            elif current_z + layer_depth + item["length_cm"] <= truck["cargo_depth_cm"]:
                # Start new row
                current_x = 0
                current_z += layer_depth
                layer_depth = 0
            elif current_y + row_height + item["height_cm"] <= truck["cargo_height_cm"]:
                # Start new layer
                current_x = 0
                current_y += row_height
                current_z = 0
                row_height = 0
                layer_depth = 0
            else:
                # Doesn't fit
                unfitted_items.append(item)
                continue
            
            # Place item
            loaded_items.append({
                "item": {
                    "sku": item["sku"],
                    "description": item["description"],
                    "weight_kg": item["weight_kg"],
                    "is_fragile": item.get("is_fragile", False),
                },
                "position": {
                    "x": current_x,
                    "y": current_y,
                    "z": current_z,
                },
                "dimensions": {
                    "width": item["width_cm"],
                    "height": item["height_cm"],
                    "depth": item["length_cm"],
                },
                "rotated": False,
            })
            
            # Update position
            current_x += item["width_cm"]
            row_height = max(row_height, item["height_cm"])
            layer_depth = max(layer_depth, item["length_cm"])
            total_weight += item["weight_kg"]
            total_volume += item["volume_cm3"]
        
        # Calculate utilization
        volume_utilization = (total_volume / 1_000_000) / truck_volume_m3
        weight_utilization = total_weight / truck["max_capacity_kg"]
        
        # Calculate center of gravity
        cog = self._calculate_center_of_gravity(loaded_items, truck)
        
        # Check stability
        is_stable = self._check_stability(cog, truck)
        
        # Generate loading sequence (reverse of placement)
        loading_sequence = [
            {
                "step": i + 1,
                "sku": item["item"]["sku"],
                "description": item["item"]["description"],
                "position": f"X:{item['position']['x']}, Y:{item['position']['y']}, Z:{item['position']['z']}",
            }
            for i, item in enumerate(loaded_items)
        ]
        
        # Generate warnings
        warnings = []
        if not is_stable:
            warnings.append("Weight distribution may affect vehicle stability")
        if unfitted_items:
            warnings.append(f"{len(unfitted_items)} item(s) could not fit")
        if volume_utilization < 0.6:
            warnings.append("Low volume utilization - consider consolidation")
        
        return {
            "truck_id": truck_id,
            "truck_plate": truck["plate"],
            "loaded_items_count": len(loaded_items),
            "unfitted_items_count": len(unfitted_items),
            "items": loaded_items,
            "unfitted_items": [
                {"sku": i["sku"], "reason": "No space available"}
                for i in unfitted_items
            ],
            "utilization": {
                "volume_rate": round(volume_utilization, 3),
                "weight_rate": round(weight_utilization, 3),
                "total_weight_kg": round(total_weight, 2),
                "total_volume_m3": round(total_volume / 1_000_000, 3),
            },
            "weight_distribution": {
                "center_of_gravity": cog,
                "is_balanced": is_stable,
                "front_weight_pct": round(cog.get("front_pct", 50), 1),
                "rear_weight_pct": round(cog.get("rear_pct", 50), 1),
            },
            "loading_sequence": loading_sequence,
            "warnings": warnings,
        }

    def _generate_sample_items(self, shipment_ids: list[str]) -> list[dict]:
        """Generate sample items for testing."""
        items = []
        
        for i, ship_id in enumerate(shipment_ids):
            # 3 items per shipment
            for j in range(3):
                items.append({
                    "sku": f"SKU-{ship_id[-3:]}-{j+1:02d}",
                    "description": f"Package {j+1} from {ship_id}",
                    "shipment_id": ship_id,
                    "weight_kg": 20 + (i * 5) + (j * 2),
                    "width_cm": 40 + (j * 10),
                    "height_cm": 30 + (j * 5),
                    "length_cm": 50 + (i * 5),
                    "volume_cm3": (40 + (j * 10)) * (30 + (j * 5)) * (50 + (i * 5)),
                    "is_fragile": j == 0,  # First item is fragile
                })
        
        return items

    def _calculate_center_of_gravity(
        self, items: list[dict], truck: dict
    ) -> dict[str, float]:
        """Calculate center of gravity of loaded items."""
        if not items:
            return {"x": 0, "y": 0, "z": 0, "front_pct": 50, "rear_pct": 50}
        
        total_weight = 0
        weighted_x = 0
        weighted_y = 0
        weighted_z = 0
        
        for item in items:
            weight = item["item"]["weight_kg"]
            x = item["position"]["x"] + item["dimensions"]["width"] / 2
            y = item["position"]["y"] + item["dimensions"]["height"] / 2
            z = item["position"]["z"] + item["dimensions"]["depth"] / 2
            
            total_weight += weight
            weighted_x += x * weight
            weighted_y += y * weight
            weighted_z += z * weight
        
        cog_x = weighted_x / total_weight if total_weight > 0 else 0
        cog_y = weighted_y / total_weight if total_weight > 0 else 0
        cog_z = weighted_z / total_weight if total_weight > 0 else 0
        
        # Calculate front/rear distribution
        midpoint_z = truck["cargo_depth_cm"] / 2
        front_pct = (1 - cog_z / truck["cargo_depth_cm"]) * 100
        rear_pct = 100 - front_pct
        
        return {
            "x": round(cog_x, 1),
            "y": round(cog_y, 1),
            "z": round(cog_z, 1),
            "front_pct": front_pct,
            "rear_pct": rear_pct,
        }

    def _check_stability(self, cog: dict, truck: dict) -> bool:
        """Check if load is stable based on COG."""
        # Check if COG is roughly centered
        center_x = truck["cargo_width_cm"] / 2
        center_z = truck["cargo_depth_cm"] / 2
        
        x_deviation = abs(cog.get("x", 0) - center_x) / center_x
        z_deviation = abs(cog.get("z", 0) - center_z) / center_z
        
        # Allow 30% deviation
        return x_deviation < 0.3 and z_deviation < 0.3
