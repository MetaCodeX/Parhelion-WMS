# ===================================
# PARHELION API - Dockerfile
# Multi-stage build para producción
# ===================================

# --- STAGE 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto primero (cache de dependencias)
COPY src/Parhelion.Domain/*.csproj ./Parhelion.Domain/
COPY src/Parhelion.Application/*.csproj ./Parhelion.Application/
COPY src/Parhelion.Infrastructure/*.csproj ./Parhelion.Infrastructure/
COPY src/Parhelion.API/*.csproj ./Parhelion.API/

# Restaurar dependencias
RUN dotnet restore Parhelion.API/Parhelion.API.csproj

# Copiar todo el código fuente
COPY src/ ./

# Build de la aplicación
RUN dotnet publish Parhelion.API/Parhelion.API.csproj -c Release -o /app/publish --no-restore

# --- STAGE 2: Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Crear usuario no-root para seguridad
RUN adduser --disabled-password --gecos '' appuser
USER appuser

# Copiar artefactos del build
COPY --from=build /app/publish .

# Puerto por defecto
EXPOSE 5000

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Comando de inicio
ENTRYPOINT ["dotnet", "Parhelion.API.dll"]
