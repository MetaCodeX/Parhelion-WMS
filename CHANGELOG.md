# Changelog

Historial de cambios del proyecto Parhelion Logistics.

---

## [Unreleased] - En desarrollo

### Pendiente

- PWA Service Workers para modo offline
- Endpoints CRUD para todas las entidades

---

## [0.4.2] - 2025-12-13

### Agregado

- **Sistema de Autenticación JWT**:

  - `AuthController`: `/login`, `/refresh`, `/logout`, `/me`
  - Access token (2h) + Refresh token (7 días)
  - Revocación de tokens, tracking de IP

- **Autorización por Roles (Inmutable)**:

  - `RolePermissions.cs` con 60+ permisos en código
  - Roles: Admin, Driver, Warehouse, DemoUser
  - Permisos NO modificables en runtime

- **Nueva Tabla `Clients`** (Remitentes/Destinatarios):

  - Datos: CompanyName, ContactName, Email, Phone
  - Fiscales: TaxId (RFC), LegalName, BillingAddress
  - Prioridad: Normal, Low, High, Urgent
  - FK en Shipment: SenderId, RecipientClientId

- **Nueva Tabla `RefreshTokens`**:

  - Token hasheado, expiración, revocación, IP, UserAgent

- **Campos Legales en `Drivers`**:

  - RFC, NSS, CURP, LicenseType, LicenseExpiration
  - EmergencyContact, EmergencyPhone, HireDate

- **Campos Legales en `Trucks`**:

  - VIN, EngineNumber, Year, Color, seguro, verificación

- **Trazabilidad en `ShipmentCheckpoints`**:
  - HandledByDriverId, LoadedOntoTruckId, ActionType

### Migración

- `AddAuthAndClients` aplicada a PostgreSQL

---

## [0.4.0] - 2025-12-12

### Agregado

- **Domain Layer Completo**: 14 entidades según `database-schema.md`
  - Core: Tenant, User, Role
  - Flotilla: Driver, Truck, FleetLog
  - Red Logística: Location, NetworkLink, RouteBlueprint, RouteStep
  - Envíos: Shipment, ShipmentItem, ShipmentCheckpoint, ShipmentDocument
- **11 Enums**: ShipmentStatus, TruckType, LocationType, CheckpointStatus, etc.
- **Infrastructure Layer con EF Core**:
  - DbContext con Query Filters globales (multi-tenancy + soft delete)
  - 14 configuraciones Fluent API con índices y constraints
  - Audit Trail automático (CreatedAt, UpdatedAt, DeletedAt)
- **Migración Inicial**: `InitialCreate` aplicada a PostgreSQL
- **Seed Data**: Roles del sistema (Admin, Driver, Warehouse, DemoUser)
- **Endpoint `/health/db`**: Verificación de estado de base de datos

### Metodología de Implementación

| Aspecto               | Implementación                                  |
| --------------------- | ----------------------------------------------- |
| **Approach**          | Code First con Entity Framework Core 8.0.10     |
| **Database**          | PostgreSQL 17 (Docker)                          |
| **Naming Convention** | PascalCase en C#, preservado en PostgreSQL      |
| **Architecture**      | Clean Architecture + Domain-Driven Design (DDD) |
| **Multi-Tenancy**     | Query Filters globales por TenantId             |
| **Soft Delete**       | IsDeleted flag en todas las entidades           |
| **Audit Trail**       | CreatedAt, UpdatedAt, DeletedAt automáticos     |

### Seguridad

- **Anti SQL Injection**: Queries parameterizadas automáticas de EF Core
- **Tenant Isolation**: Query Filters globales por TenantId
- **Soft Delete**: Todas las entidades soportan borrado lógico
- **Password Strategy**: BCrypt (usuarios) + Argon2id (admins)

### Configurado

- **Connection Strings**: Separación develop/production
- **Paquetes NuGet**:
  - Npgsql.EntityFrameworkCore.PostgreSQL 8.0.10
  - Microsoft.EntityFrameworkCore.Design 8.0.10

---

## [0.3.0] - 2025-12-12

### Agregado

- **Sistema de Diseño Neo-Brutalism**: Estilo visual moderno con bordes sólidos y sombras
  - Paleta "Industrial Solar": Oxide (#C85A17), Sand (#E8E6E1), Black (#000000)
  - Tipografía: New Rocker (logo), Merriweather (títulos), Inter (body)
  - Componentes: Buttons, Cards, Inputs con estilo brutalist
- **Grid Animado**: Fondo con grid cuadriculado naranja y movimiento aleatorio
  - Dirección random en cada carga de página
  - 8 direcciones posibles (cardinales + diagonales)
- **Remote Development**: Frontends configurados para acceso via Tailscale
  - Vite servers escuchando en `0.0.0.0`
  - Backend API accesible remotamente

### Configurado

- **Puertos dedicados** via `.env`:
  - Backend: 5100
  - Admin: 4100
  - Operaciones: 5101
  - Campo: 5102
- **Endpoint `/health`** en backend API para verificación de estado

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

1. ~~Implementar Domain Layer (entidades)~~ ✅
2. ~~Configurar Infrastructure Layer (EF Core)~~ ✅
3. Crear API endpoints básicos (CRUD)
4. Implementar autenticación JWT
5. Diseñar UI del Admin
6. Probar Docker con PostgreSQL
