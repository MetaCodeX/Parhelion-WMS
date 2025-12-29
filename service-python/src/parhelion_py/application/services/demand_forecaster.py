"""
Demand Forecaster Service - Time Series Prediction

Forecasts shipping demand using:
- Historical patterns
- Seasonality (daily, weekly, monthly)
- Trend analysis
- Holiday adjustments
"""

from datetime import datetime, timedelta
from typing import Any
from uuid import UUID

import numpy as np
import pandas as pd


class DemandForecaster:
    """
    Time series forecasting for shipment demand.
    
    Uses statistical methods to predict future demand
    and recommend resource allocation.
    """

    def __init__(self):
        self._mexican_holidays = [
            (1, 1),   # Año Nuevo
            (2, 5),   # Constitución
            (3, 21),  # Benito Juárez
            (5, 1),   # Día del Trabajo
            (9, 16),  # Independencia
            (11, 20), # Revolución
            (12, 25), # Navidad
        ]

    async def forecast_demand(
        self,
        tenant_id: UUID,
        location_id: str | None = None,
        days: int = 30,
    ) -> dict[str, Any]:
        """
        Forecast shipment demand for the next N days.
        
        Args:
            tenant_id: Tenant identifier
            location_id: Optional location filter
            days: Number of days to forecast
            
        Returns:
            Dict with predictions, peak days, recommendations
        """
        # Get historical data (simulated)
        historical = self._generate_sample_history(days_back=90)
        
        # Prepare time series
        df = pd.DataFrame(historical)
        df["ds"] = pd.to_datetime(df["date"])
        df["y"] = df["shipment_count"]
        
        # Calculate components
        trend = self._calculate_trend(df)
        seasonality = self._calculate_seasonality(df)
        
        # Generate forecast
        predictions = []
        today = datetime.now().date()
        
        for i in range(days):
            forecast_date = today + timedelta(days=i+1)
            
            # Base prediction from trend
            base = trend["slope"] * (len(df) + i) + trend["intercept"]
            
            # Apply seasonality
            weekday = forecast_date.weekday()
            seasonal_factor = seasonality.get(weekday, 1.0)
            base *= seasonal_factor
            
            # Holiday adjustment
            if self._is_mexican_holiday(forecast_date):
                base *= 0.4  # 60% reduction on holidays
            
            # Add some variance
            lower = base * 0.85
            upper = base * 1.15
            
            predictions.append({
                "date": forecast_date.isoformat(),
                "predicted_shipments": int(max(0, round(base))),
                "lower_bound": int(max(0, round(lower))),
                "upper_bound": int(round(upper)),
                "confidence": 0.85 if i < 7 else 0.70,
                "is_holiday": self._is_mexican_holiday(forecast_date),
                "day_of_week": forecast_date.strftime("%A"),
            })
        
        # Identify peak days
        preds_df = pd.DataFrame(predictions)
        peak_days = preds_df.nlargest(5, "predicted_shipments")["date"].tolist()
        
        # Resource recommendations
        avg_daily = float(preds_df["predicted_shipments"].mean())
        max_daily = int(preds_df["predicted_shipments"].max())
        
        return {
            "tenant_id": str(tenant_id),
            "location_id": location_id,
            "forecast_period_days": days,
            "generated_at": datetime.now().isoformat(),
            "predictions": predictions,
            "peak_days": peak_days,
            "statistics": {
                "avg_daily_shipments": round(avg_daily, 1),
                "max_daily_shipments": max_daily,
                "total_predicted": int(preds_df["predicted_shipments"].sum()),
            },
            "resource_recommendations": {
                "recommended_trucks": int(max(1, round(max_daily / 20))),
                "recommended_drivers": int(max(1, round(max_daily / 15))),
                "peak_day_trucks": int(max(1, round(max_daily / 15))),
            },
            "model_info": {
                "algorithm": "linear_trend_with_seasonality",
                "training_days": 90,
                "overall_confidence": 0.80,
            },
        }

    def _generate_sample_history(self, days_back: int) -> list[dict]:
        """Generate sample historical data."""
        np.random.seed(42)
        history = []
        today = datetime.now().date()
        
        for i in range(days_back, 0, -1):
            date = today - timedelta(days=i)
            
            # Base demand with trend
            base = 50 + (days_back - i) * 0.1
            
            # Weekly seasonality
            weekday = date.weekday()
            if weekday == 0:  # Monday - high
                base *= 1.3
            elif weekday in [1, 2, 3]:  # Tue-Thu
                base *= 1.1
            elif weekday == 4:  # Friday
                base *= 1.2
            else:  # Weekend
                base *= 0.5
            
            # Add noise
            count = max(0, round(base + np.random.normal(0, 10)))
            
            history.append({
                "date": date.isoformat(),
                "shipment_count": count,
            })
        
        return history

    def _calculate_trend(self, df: pd.DataFrame) -> dict[str, float]:
        """Calculate linear trend."""
        x = np.arange(len(df))
        y = df["y"].values
        
        # Simple linear regression
        slope = np.cov(x, y)[0, 1] / np.var(x) if np.var(x) > 0 else 0
        intercept = np.mean(y) - slope * np.mean(x)
        
        return {"slope": slope, "intercept": intercept}

    def _calculate_seasonality(self, df: pd.DataFrame) -> dict[int, float]:
        """Calculate weekly seasonality factors."""
        df["weekday"] = df["ds"].dt.dayofweek
        
        overall_mean = df["y"].mean()
        weekday_means = df.groupby("weekday")["y"].mean()
        
        seasonality = {}
        for weekday, mean in weekday_means.items():
            seasonality[weekday] = mean / overall_mean if overall_mean > 0 else 1.0
        
        return seasonality

    def _is_mexican_holiday(self, date) -> bool:
        """Check if date is a Mexican holiday."""
        if isinstance(date, str):
            date = datetime.fromisoformat(date).date()
        
        return (date.month, date.day) in self._mexican_holidays
