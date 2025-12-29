"""
Parhelion Python Analytics Service - Main Application
======================================================

FastAPI application entry point with Clean Architecture.
"""

import logging
import sys
from contextlib import asynccontextmanager
from typing import AsyncGenerator

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from parhelion_py.api.routers import health, analytics
from parhelion_py.infrastructure.config.settings import get_settings

settings = get_settings()


def configure_logging() -> None:
    """Configure structured JSON logging for production."""
    log_level = logging.DEBUG if settings.environment == "development" else logging.INFO
    
    # JSON formatter for production
    if settings.environment == "production":
        logging.basicConfig(
            level=log_level,
            format='{"timestamp": "%(asctime)s", "level": "%(levelname)s", "logger": "%(name)s", "message": "%(message)s"}',
            datefmt='%Y-%m-%dT%H:%M:%S',
            stream=sys.stdout,
        )
    else:
        logging.basicConfig(
            level=log_level,
            format='%(asctime)s | %(levelname)-8s | %(name)s | %(message)s',
            datefmt='%H:%M:%S',
            stream=sys.stdout,
        )
    
    # Reduce noise from third-party libraries
    logging.getLogger("httpx").setLevel(logging.WARNING)
    logging.getLogger("httpcore").setLevel(logging.WARNING)
    logging.getLogger("uvicorn.access").setLevel(logging.INFO)


@asynccontextmanager
async def lifespan(app: FastAPI) -> AsyncGenerator[None, None]:
    """Application lifespan handler for startup/shutdown events."""
    configure_logging()
    logger = logging.getLogger(__name__)
    
    # Startup
    logger.info(f"Starting Parhelion Python Analytics v{settings.version}")
    logger.info(f"Environment: {settings.environment}")
    logger.info(f"API URL: {settings.parhelion_api_url}")
    
    yield
    
    # Shutdown
    logger.info("Shutting down Parhelion Python Analytics")


def create_app() -> FastAPI:
    """Factory function to create FastAPI application."""
    
    app = FastAPI(
        title="Parhelion Python Analytics",
        description="Microservicio de análisis y predicciones para Parhelion Logistics",
        version=settings.version,
        docs_url="/docs" if settings.environment != "production" else None,
        redoc_url="/redoc" if settings.environment != "production" else None,
        lifespan=lifespan,
    )
    
    # CORS Middleware
    app.add_middleware(
        CORSMiddleware,
        allow_origins=settings.cors_origins,
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )
    
    # Include routers
    app.include_router(health.router, tags=["Health"])
    app.include_router(analytics.router)
    
    return app


# Application instance
app = create_app()


if __name__ == "__main__":
    import uvicorn
    
    uvicorn.run(
        "parhelion_py.main:app",
        host="0.0.0.0",
        port=8000,
        reload=settings.environment == "development",
    )

