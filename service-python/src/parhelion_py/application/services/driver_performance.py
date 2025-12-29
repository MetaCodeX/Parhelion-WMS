"""
Driver Performance Service - Analytics

Analyzes driver performance metrics:
- On-time delivery rate
- Average delay
- Exception count
- Ranking vs peers
"""

from datetime import datetime, timedelta
from typing import Any
from uuid import UUID

import numpy as np
import pandas as pd


class DriverPerformanceAnalyzer:
    """
    Driver performance analytics service.
    
    Calculates individual and comparative metrics
    for driver performance evaluation.
    """

    def __init__(self):
        pass

    async def analyze_driver(
        self,
        tenant_id: UUID,
        driver_id: str,
        days_back: int = 30,
    ) -> dict[str, Any]:
        """
        Analyze driver performance.
        
        Args:
            tenant_id: Tenant identifier
            driver_id: Driver to analyze
            days_back: Days of history to analyze
            
        Returns:
            Dict with performance metrics and ranking
        """
        # Get driver data (simulated)
        driver = self._get_sample_driver(driver_id)
        deliveries = self._get_sample_deliveries(driver_id, days_back)
        
        df = pd.DataFrame(deliveries)
        
        # Calculate metrics
        total_deliveries = len(df)
        
        if total_deliveries == 0:
            return {
                "driver_id": driver_id,
                "error": "No deliveries found",
            }
        
        # On-time rate
        on_time = df[df["is_on_time"] == True]
        on_time_rate = len(on_time) / total_deliveries * 100
        
        # Average delay (for late deliveries)
        late = df[df["delay_minutes"] > 0]
        avg_delay = late["delay_minutes"].mean() if len(late) > 0 else 0
        
        # Exceptions
        exceptions = df[df["is_exception"] == True]
        exception_count = len(exceptions)
        
        # Missing checkpoints
        missing_checkpoints = df["missing_checkpoints"].sum()
        
        # Get peer comparison
        all_drivers = self._get_all_driver_stats(tenant_id, days_back)
        percentile = self._calculate_percentile(on_time_rate, all_drivers)
        
        # Calculate overall rating (1-5 stars)
        rating = self._calculate_rating(on_time_rate, avg_delay, exception_count)
        
        # Trend analysis
        trend = self._analyze_trend(df)
        
        return {
            "driver_id": driver_id,
            "driver_name": driver["name"],
            "analysis_period_days": days_back,
            "generated_at": datetime.now().isoformat(),
            "metrics": {
                "total_deliveries": int(total_deliveries),
                "on_time_rate": float(round(on_time_rate, 1)),
                "avg_delay_minutes": float(round(avg_delay, 1)) if avg_delay else 0.0,
                "exception_count": int(exception_count),
                "missing_checkpoints": int(missing_checkpoints),
            },
            "rating": {
                "stars": float(rating),
                "category": self._rating_category(rating),
            },
            "comparison": {
                "percentile_rank": int(percentile),
                "is_top_performer": bool(percentile >= 90),
                "vs_average": float(round(on_time_rate - all_drivers["avg"], 1)),
            },
            "trend": trend,
            "recommendations": self._generate_recommendations(
                on_time_rate, avg_delay, exception_count, missing_checkpoints
            ),
        }

    async def get_leaderboard(
        self,
        tenant_id: UUID,
        days_back: int = 30,
        limit: int = 10,
    ) -> list[dict[str, Any]]:
        """
        Get driver performance leaderboard.
        
        Args:
            tenant_id: Tenant identifier
            days_back: Days of history
            limit: Number of drivers to return
            
        Returns:
            List of top performing drivers
        """
        # Generate sample leaderboard
        np.random.seed(42)
        
        drivers = []
        for i in range(20):
            on_time_rate = np.random.uniform(70, 99)
            avg_delay = np.random.uniform(0, 30)
            rating = self._calculate_rating(on_time_rate, avg_delay, 0)
            
            drivers.append({
                "rank": 0,  # Will be assigned
                "driver_id": f"driver-{i:03d}",
                "driver_name": f"Driver {i+1}",
                "on_time_rate": float(round(on_time_rate, 1)),
                "avg_delay_minutes": float(round(avg_delay, 1)),
                "total_deliveries": int(np.random.randint(50, 200)),
                "rating": float(rating),
            })
        
        # Sort by on-time rate
        drivers.sort(key=lambda x: x["on_time_rate"], reverse=True)
        
        # Assign ranks
        for i, driver in enumerate(drivers):
            driver["rank"] = i + 1
        
        return drivers[:limit]

    def _get_sample_driver(self, driver_id: str) -> dict:
        """Get sample driver data."""
        return {
            "id": driver_id,
            "name": f"Juan Pérez",
            "license": "LIC-12345",
            "status": "Active",
        }

    def _get_sample_deliveries(self, driver_id: str, days: int) -> list[dict]:
        """Generate sample deliveries."""
        np.random.seed(hash(driver_id) % 1000)
        
        deliveries = []
        base_on_time_rate = np.random.uniform(0.75, 0.95)
        
        for i in range(days * 3):  # ~3 deliveries per day
            is_on_time = np.random.random() < base_on_time_rate
            
            deliveries.append({
                "id": f"del-{i:04d}",
                "date": (datetime.now() - timedelta(days=i // 3)).isoformat(),
                "is_on_time": is_on_time,
                "delay_minutes": 0 if is_on_time else np.random.uniform(5, 60),
                "is_exception": np.random.random() < 0.02,
                "missing_checkpoints": 1 if np.random.random() < 0.05 else 0,
            })
        
        return deliveries

    def _get_all_driver_stats(self, tenant_id: UUID, days: int) -> dict:
        """Get aggregate stats for all drivers."""
        return {
            "avg": 85.0,  # Average on-time rate
            "median": 87.0,
            "min": 65.0,
            "max": 98.0,
        }

    def _calculate_percentile(self, value: float, stats: dict) -> int:
        """Calculate percentile rank."""
        if value >= stats["max"]:
            return 99
        if value <= stats["min"]:
            return 1
        
        range_val = stats["max"] - stats["min"]
        position = (value - stats["min"]) / range_val
        return int(position * 100)

    def _calculate_rating(
        self, on_time_rate: float, avg_delay: float, exceptions: int
    ) -> float:
        """Calculate star rating (1-5)."""
        # Base rating from on-time rate
        if on_time_rate >= 95:
            base = 5.0
        elif on_time_rate >= 90:
            base = 4.5
        elif on_time_rate >= 85:
            base = 4.0
        elif on_time_rate >= 80:
            base = 3.5
        elif on_time_rate >= 75:
            base = 3.0
        else:
            base = 2.5
        
        # Penalties
        delay_penalty = min(0.5, avg_delay / 60)
        exception_penalty = min(0.5, exceptions * 0.1)
        
        return max(1.0, round(base - delay_penalty - exception_penalty, 1))

    def _rating_category(self, rating: float) -> str:
        """Get rating category."""
        if rating >= 4.5:
            return "EXCELLENT"
        elif rating >= 4.0:
            return "GOOD"
        elif rating >= 3.0:
            return "AVERAGE"
        elif rating >= 2.0:
            return "BELOW_AVERAGE"
        else:
            return "NEEDS_IMPROVEMENT"

    def _analyze_trend(self, df: pd.DataFrame) -> dict:
        """Analyze performance trend."""
        if len(df) < 10:
            return {"direction": "STABLE", "change_pct": 0}
        
        # Split into two halves
        mid = len(df) // 2
        first_half = df.iloc[:mid]["is_on_time"].mean()
        second_half = df.iloc[mid:]["is_on_time"].mean()
        
        change = (second_half - first_half) * 100
        
        if change > 5:
            direction = "IMPROVING"
        elif change < -5:
            direction = "DECLINING"
        else:
            direction = "STABLE"
        
        return {
            "direction": direction,
            "change_pct": round(change, 1),
        }

    def _generate_recommendations(
        self,
        on_time_rate: float,
        avg_delay: float,
        exceptions: int,
        missing_checkpoints: int,
    ) -> list[str]:
        """Generate improvement recommendations."""
        recommendations = []
        
        if on_time_rate < 85:
            recommendations.append("Focus on route planning to improve on-time delivery")
        
        if avg_delay > 30:
            recommendations.append("Review causes of delays and address bottlenecks")
        
        if exceptions > 5:
            recommendations.append("Investigate recurring exception patterns")
        
        if missing_checkpoints > 3:
            recommendations.append("Ensure consistent checkpoint scanning")
        
        if not recommendations:
            recommendations.append("Excellent performance - keep up the good work!")
        
        return recommendations
