"""
Authentication middleware for Python Analytics Service.

Validates requests from:
1. .NET API using X-Internal-Service-Key header
2. n8n/external using JWT Bearer token
"""

from typing import Annotated

from fastapi import Depends, Header, HTTPException, status
from jose import JWTError, jwt

from parhelion_py.infrastructure.config.settings import Settings, get_settings


async def verify_internal_service_key(
    x_internal_service_key: Annotated[str | None, Header()] = None,
    settings: Settings = Depends(get_settings),
) -> bool:
    """
    Verify X-Internal-Service-Key header for .NET API calls.
    
    This is the primary authentication method for inter-service
    communication within the Docker network.
    """
    if not x_internal_service_key:
        return False
    
    return x_internal_service_key == settings.internal_service_key


async def verify_jwt_token(
    authorization: Annotated[str | None, Header()] = None,
    settings: Settings = Depends(get_settings),
) -> dict | None:
    """
    Verify JWT Bearer token for n8n/external calls.
    
    Returns the decoded token payload if valid, None otherwise.
    """
    if not authorization:
        return None
    
    if not authorization.startswith("Bearer "):
        return None
    
    token = authorization[7:]  # Remove "Bearer " prefix
    
    try:
        payload = jwt.decode(
            token,
            settings.jwt_secret,
            algorithms=["HS256"],
            options={"verify_aud": False},
        )
        return payload
    except JWTError:
        return None


async def require_authentication(
    internal_key_valid: bool = Depends(verify_internal_service_key),
    jwt_payload: dict | None = Depends(verify_jwt_token),
) -> dict:
    """
    Require at least one valid authentication method.
    
    Raises HTTPException 401 if neither internal key nor JWT is valid.
    
    Returns:
        Authentication context with source and claims
    """
    if internal_key_valid:
        return {
            "authenticated": True,
            "source": "internal_service",
            "claims": {"service": "parhelion-api"},
        }
    
    if jwt_payload:
        return {
            "authenticated": True,
            "source": "jwt",
            "claims": jwt_payload,
        }
    
    raise HTTPException(
        status_code=status.HTTP_401_UNAUTHORIZED,
        detail="Invalid or missing authentication credentials",
        headers={"WWW-Authenticate": "Bearer"},
    )


class AuthContext:
    """Authentication context for dependency injection."""
    
    def __init__(self, auth_data: dict):
        self.authenticated = auth_data.get("authenticated", False)
        self.source = auth_data.get("source", "unknown")
        self.claims = auth_data.get("claims", {})
    
    @property
    def is_internal_service(self) -> bool:
        return self.source == "internal_service"
    
    @property
    def is_jwt(self) -> bool:
        return self.source == "jwt"
    
    @property
    def tenant_id(self) -> str | None:
        """Extract tenant_id from JWT claims if present."""
        return self.claims.get("tenant_id") or self.claims.get("tenantId")
    
    @property
    def user_id(self) -> str | None:
        """Extract user_id from JWT claims if present."""
        return self.claims.get("sub") or self.claims.get("user_id")


async def get_auth_context(
    auth_data: dict = Depends(require_authentication),
) -> AuthContext:
    """Get typed authentication context."""
    return AuthContext(auth_data)


# Type alias for cleaner dependency injection
AuthContextDep = Annotated[AuthContext, Depends(get_auth_context)]
