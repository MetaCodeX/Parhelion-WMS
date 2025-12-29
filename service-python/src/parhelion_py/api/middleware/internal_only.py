"""
Internal-only middleware for Python Analytics Service.

This service is INTERNAL and should only accept calls from
the .NET API within the Docker network. No JWT validation needed.
"""

import logging
from typing import Annotated

from fastapi import Depends, Header, HTTPException, status, Request

logger = logging.getLogger(__name__)


async def verify_internal_origin(request: Request) -> bool:
    """
    Verify the request comes from internal Docker network.
    
    In production, this could check:
    - X-Internal-Call header (optional extra security)
    - Source IP is within Docker network range
    
    For now, we trust the Docker network isolation.
    """
    # Log internal call for debugging
    client_host = request.client.host if request.client else "unknown"
    logger.debug(f"Internal call from: {client_host}")
    
    # Optional: Check for internal header (defense in depth)
    internal_header = request.headers.get("X-Internal-Call")
    if internal_header:
        logger.debug("X-Internal-Call header present")
    
    return True


async def require_internal_call(
    is_internal: bool = Depends(verify_internal_origin),
) -> bool:
    """
    Dependency to mark endpoints as internal-only.
    
    Currently trusts Docker network isolation.
    Can be enhanced to validate specific headers or IPs.
    """
    if not is_internal:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="This endpoint is internal only",
        )
    return True


# Type alias for dependency injection
InternalCallDep = Annotated[bool, Depends(require_internal_call)]
