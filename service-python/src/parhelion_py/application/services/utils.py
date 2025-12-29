"""
Utility functions for serialization and data conversion.
"""

import numpy as np
from datetime import datetime, date
from typing import Any


def convert_numpy_types(obj: Any) -> Any:
    """
    Recursively convert numpy types to Python native types for JSON serialization.
    
    Args:
        obj: Object that may contain numpy types
        
    Returns:
        Object with all numpy types converted to Python native types
    """
    if isinstance(obj, dict):
        return {k: convert_numpy_types(v) for k, v in obj.items()}
    elif isinstance(obj, list):
        return [convert_numpy_types(item) for item in obj]
    elif isinstance(obj, tuple):
        return tuple(convert_numpy_types(item) for item in obj)
    elif isinstance(obj, np.integer):
        return int(obj)
    elif isinstance(obj, np.floating):
        return float(obj)
    elif isinstance(obj, np.bool_):
        return bool(obj)
    elif isinstance(obj, np.ndarray):
        return obj.tolist()
    elif isinstance(obj, (datetime, date)):
        return obj.isoformat()
    elif hasattr(obj, 'item'):  # numpy scalar
        return obj.item()
    return obj


def safe_round(value: Any, decimals: int = 2) -> float | int:
    """
    Safely round a value and convert to Python float.
    
    Args:
        value: Value to round (may be numpy type)
        decimals: Number of decimal places
        
    Returns:
        Rounded Python float
    """
    if isinstance(value, (np.integer, np.floating)):
        value = float(value)
    return round(float(value), decimals) if decimals > 0 else int(round(value))
