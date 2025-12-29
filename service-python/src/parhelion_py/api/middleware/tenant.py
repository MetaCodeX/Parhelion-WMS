"""
Multi-tenant middleware for Python Analytics Service.

Extracts and validates tenant context from requests, ensuring
proper data isolation across tenants.
"""

from typing import Annotated
from uuid import UUID

from fastapi import Depends, Header, HTTPException, status

from parhelion_py.api.middleware.auth import AuthContextDep


class TenantContext:
    """
    Tenant context for multi-tenant data isolation.
    
    Carries the current tenant ID throughout the request lifecycle,
    ensuring all database queries and API calls are properly scoped.
    """
    
    def __init__(self, tenant_id: UUID):
        self._tenant_id = tenant_id
    
    @property
    def id(self) -> UUID:
        return self._tenant_id
    
    def __str__(self) -> str:
        return str(self._tenant_id)


async def get_tenant_context(
    x_tenant_id: Annotated[str | None, Header()] = None,
    auth_context: AuthContextDep = None,
) -> TenantContext:
    """
    Extract tenant context from request.
    
    Tries to get tenant ID from:
    1. X-Tenant-Id header (from .NET API calls)
    2. JWT claims (from n8n/external calls)
    
    Raises HTTPException 400 if tenant cannot be determined.
    """
    tenant_id_str: str | None = None
    
    # Priority 1: Explicit header
    if x_tenant_id:
        tenant_id_str = x_tenant_id
    
    # Priority 2: JWT claims
    elif auth_context and auth_context.tenant_id:
        tenant_id_str = auth_context.tenant_id
    
    if not tenant_id_str:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Tenant ID is required. Provide X-Tenant-Id header or include in JWT.",
        )
    
    try:
        tenant_uuid = UUID(tenant_id_str)
    except ValueError:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=f"Invalid tenant ID format: {tenant_id_str}",
        )
    
    return TenantContext(tenant_uuid)


# Type alias for cleaner dependency injection
TenantContextDep = Annotated[TenantContext, Depends(get_tenant_context)]


async def optional_tenant_context(
    x_tenant_id: Annotated[str | None, Header()] = None,
    auth_context: AuthContextDep = None,
) -> TenantContext | None:
    """
    Optional tenant context for endpoints that don't require tenant.
    
    Returns None if tenant cannot be determined instead of raising error.
    """
    tenant_id_str: str | None = None
    
    if x_tenant_id:
        tenant_id_str = x_tenant_id
    elif auth_context and auth_context.tenant_id:
        tenant_id_str = auth_context.tenant_id
    
    if not tenant_id_str:
        return None
    
    try:
        tenant_uuid = UUID(tenant_id_str)
        return TenantContext(tenant_uuid)
    except ValueError:
        return None


OptionalTenantContextDep = Annotated[TenantContext | None, Depends(optional_tenant_context)]
