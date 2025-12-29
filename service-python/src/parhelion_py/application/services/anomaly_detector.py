"""
Anomaly Detector Service - Isolation Forest

Detects anomalies in shipment tracking:
- Unusual checkpoint patterns
- Time deviations
- Route anomalies
- Missing checkpoints
"""

from datetime import datetime
from typing import Any
from uuid import UUID

import numpy as np
import pandas as pd
from sklearn.ensemble import IsolationForest
from sklearn.preprocessing import StandardScaler


class AnomalyDetector:
    """
    ML-based anomaly detection for shipment tracking.
    
    Uses Isolation Forest to identify unusual patterns
    in checkpoint data and shipment behavior.
    """

    def __init__(self):
        self._model = IsolationForest(
            contamination=0.05,  # Expect 5% anomalies
            random_state=42,
            n_estimators=100,
        )
        self._scaler = StandardScaler()

    async def detect_anomalies(
        self,
        tenant_id: UUID,
        hours_back: int = 24,
        severity_filter: str | None = None,
    ) -> list[dict[str, Any]]:
        """
        Detect anomalies in recent shipment activity.
        
        Args:
            tenant_id: Tenant identifier
            hours_back: Hours of history to analyze
            severity_filter: Optional filter for severity level
            
        Returns:
            List of detected anomalies with details
        """
        # Get sample data (in production, fetch from API)
        shipments = self._generate_sample_data()
        
        if not shipments:
            return []
        
        df = pd.DataFrame(shipments)
        
        # Feature engineering
        features = self._extract_features(df)
        
        if features.empty:
            return []
        
        # Normalize features
        features_scaled = self._scaler.fit_transform(features)
        
        # Detect anomalies
        predictions = self._model.fit_predict(features_scaled)
        scores = self._model.score_samples(features_scaled)
        
        # Filter anomalies
        anomaly_mask = predictions == -1
        anomaly_indices = df.index[anomaly_mask]
        
        anomalies = []
        for idx in anomaly_indices:
            row = df.loc[idx]
            score = scores[idx]
            
            # Determine severity
            severity = self._calculate_severity(score)
            
            if severity_filter and severity != severity_filter:
                continue
            
            # Classify anomaly type
            anomaly_type = self._classify_anomaly(row, features.loc[idx])
            
            anomalies.append({
                "id": f"anomaly-{idx}",
                "shipment_id": row.get("shipment_id"),
                "tracking_number": row.get("tracking_number"),
                "detected_at": datetime.now().isoformat(),
                "anomaly_type": anomaly_type,
                "severity": severity,
                "anomaly_score": round(float(score), 3),
                "description": self._generate_description(anomaly_type, row),
                "suggested_actions": self._generate_actions(anomaly_type),
                "metrics": {
                    "checkpoint_gap_hours": float(row.get("checkpoint_gap_hours", 0)),
                    "eta_deviation_hours": float(row.get("eta_deviation_hours", 0)),
                    "sequence_valid": bool(row.get("sequence_valid", True)),
                },
            })
        
        # Sort by severity
        severity_order = {"CRITICAL": 0, "WARNING": 1, "INFO": 2}
        anomalies.sort(key=lambda x: severity_order.get(x["severity"], 3))
        
        return anomalies

    def _generate_sample_data(self) -> list[dict]:
        """Generate sample shipment data for testing."""
        np.random.seed(42)
        data = []
        
        for i in range(50):
            is_anomaly = i < 5  # First 5 are anomalies
            
            data.append({
                "shipment_id": f"ship-{i:03d}",
                "tracking_number": f"TRX-2025-{i:03d}",
                "checkpoint_count": 8 if not is_anomaly else np.random.randint(2, 4),
                "checkpoint_gap_hours": 2.5 if not is_anomaly else np.random.uniform(8, 24),
                "eta_deviation_hours": np.random.normal(0, 0.5) if not is_anomaly else np.random.uniform(4, 12),
                "sequence_valid": True if not is_anomaly else np.random.choice([True, False]),
                "unique_locations": 4 if not is_anomaly else np.random.randint(1, 3),
                "status": "InTransit" if not is_anomaly else "Exception",
            })
        
        return data

    def _extract_features(self, df: pd.DataFrame) -> pd.DataFrame:
        """Extract features for anomaly detection."""
        features = pd.DataFrame({
            "checkpoint_count": df.get("checkpoint_count", 0),
            "checkpoint_gap_hours": df.get("checkpoint_gap_hours", 0),
            "eta_deviation_hours": df.get("eta_deviation_hours", 0).abs(),
            "sequence_valid": df.get("sequence_valid", True).astype(int),
            "unique_locations": df.get("unique_locations", 1),
        })
        return features

    def _calculate_severity(self, score: float) -> str:
        """Calculate severity based on anomaly score."""
        if score < -0.5:
            return "CRITICAL"
        elif score < -0.3:
            return "WARNING"
        else:
            return "INFO"

    def _classify_anomaly(self, row: pd.Series, features: pd.Series) -> str:
        """Classify the type of anomaly."""
        if features.get("checkpoint_gap_hours", 0) > 6:
            return "MISSING_CHECKPOINTS"
        elif features.get("eta_deviation_hours", 0) > 4:
            return "SIGNIFICANT_DELAY"
        elif not row.get("sequence_valid", True):
            return "INVALID_SEQUENCE"
        elif features.get("unique_locations", 0) < 2:
            return "STUCK_IN_TRANSIT"
        else:
            return "UNKNOWN_PATTERN"

    def _generate_description(self, anomaly_type: str, row: pd.Series) -> str:
        """Generate human-readable description."""
        descriptions = {
            "MISSING_CHECKPOINTS": f"Shipment {row.get('tracking_number')} has not been scanned in {row.get('checkpoint_gap_hours', 0):.1f} hours",
            "SIGNIFICANT_DELAY": f"Shipment {row.get('tracking_number')} is delayed by {row.get('eta_deviation_hours', 0):.1f} hours",
            "INVALID_SEQUENCE": f"Shipment {row.get('tracking_number')} has checkpoints in unexpected order",
            "STUCK_IN_TRANSIT": f"Shipment {row.get('tracking_number')} appears to be stuck at same location",
            "UNKNOWN_PATTERN": f"Unusual activity detected for shipment {row.get('tracking_number')}",
        }
        return descriptions.get(anomaly_type, "Anomaly detected")

    def _generate_actions(self, anomaly_type: str) -> list[str]:
        """Generate suggested actions for the anomaly."""
        actions = {
            "MISSING_CHECKPOINTS": [
                "Contact driver for location update",
                "Check last known GPS position",
                "Verify with route hub",
            ],
            "SIGNIFICANT_DELAY": [
                "Notify customer of delay",
                "Check for route issues",
                "Consider expedited handling",
            ],
            "INVALID_SEQUENCE": [
                "Verify checkpoint data accuracy",
                "Check for manual scanning errors",
                "Review driver route compliance",
            ],
            "STUCK_IN_TRANSIT": [
                "Contact hub for package status",
                "Check for processing backlog",
                "Escalate to operations manager",
            ],
            "UNKNOWN_PATTERN": [
                "Review shipment history",
                "Contact assigned driver",
            ],
        }
        return actions.get(anomaly_type, ["Investigate further"])
