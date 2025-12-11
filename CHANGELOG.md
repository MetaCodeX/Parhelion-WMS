# Changelog

Historial de cambios del proyecto Parhelion Logistics.

---

## [Unreleased] - En desarrollo

### Pendiente

- Implementar entidades del Domain (Tenant, User, Shipment, etc.)
- Configurar Entity Framework Core y migraciones
- Crear endpoints de la API
- Diseñar interfaces de los frontends
- Configurar Swagger en el backend

---

## [0.2.0] - 2025-12-11

### Agregado

- **Docker**: Configuración completa de docker-compose con 6 servicios
  - PostgreSQL 16 con healthcheck
  - Backend API (.NET 8)
  - Frontend Admin (Angular 18)
  - Frontend Operaciones (React + Vite)
  - Frontend Campo (React + Vite)
  - Cloudflare Tunnel para exposición pública
- **Healthchecks**: Todos los servicios tienen verificación de salud
- **CI/CD**: Pipeline de GitHub Actions para build y test automático
- **Red Docker**: Todos los servicios en `parhelion-network`

### Configurado

- Variables de entorno via `.env` (no versionado)
- Cloudflared espera a que todos los servicios estén healthy

---

## [0.1.0] - 2025-12-11

### Agregado

- **Estructura del proyecto**: 4 carpetas principales
  - `backend/`: .NET 8 Web API con Clean Architecture
  - `frontend-admin/`: Angular 18 con routing
  - `frontend-operaciones/`: React + Vite + TypeScript
  - `frontend-campo/`: React + Vite + TypeScript
- **Documentación**:
  - `database-schema.md`: Esquema completo de BD
  - `requirments.md`: Requerimientos funcionales
  - `BRANCHING.md`: Estrategia de ramas Git Flow
- **Git Flow**: Ramas `main` y `develop` configuradas

### Notas

- Las 4 feature branches vacías fueron eliminadas
- Solo se crean branches cuando hay trabajo real

---

## Próximos Pasos

1. Implementar Domain Layer (entidades)
2. Configurar Infrastructure Layer (EF Core)
3. Crear API endpoints básicos
4. Diseñar UI del Admin
5. Probar Docker en local
