# ===================================
# PARHELION API - Dockerfile
# Multi-stage build para producción
# ===================================

# --- STAGE 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar archivo de solución
COPY Parhelion.sln ./

# Copiar todos los archivos de proyecto manteniendo estructura src/
COPY src/Parhelion.Domain/*.csproj ./src/Parhelion.Domain/
COPY src/Parhelion.Application/*.csproj ./src/Parhelion.Application/
COPY src/Parhelion.Infrastructure/*.csproj ./src/Parhelion.Infrastructure/
COPY src/Parhelion.API/*.csproj ./src/Parhelion.API/

# Copiar archivos de proyecto de tests
COPY tests/Parhelion.Tests/*.csproj ./tests/Parhelion.Tests/

# Restaurar dependencias de toda la solución
RUN dotnet restore Parhelion.sln

# Copiar todo el código fuente
COPY src/ ./src/
COPY tests/ ./tests/

# Build de la aplicación
RUN dotnet publish src/Parhelion.API/Parhelion.API.csproj -c Release -o /publish

# --- STAGE 2: Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Instalar curl para healthcheck (como root)
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Crear usuario no-root para seguridad
RUN adduser --disabled-password --gecos '' appuser
USER appuser

# Copiar artefactos del build
COPY --from=build /publish .

# Puerto por defecto
EXPOSE 5000

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Comando de inicio
ENTRYPOINT ["dotnet", "Parhelion.API.dll"]
