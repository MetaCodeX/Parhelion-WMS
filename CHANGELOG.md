# Changelog

Historial de cambios del proyecto Parhelion Logistics.

---

## [0.5.1] - 2025-12-16

### Agregado

- **Foundation Layer (Repository Pattern)**:

  - `IGenericRepository<T>` - Operaciones CRUD genericas con soft delete
  - `ITenantRepository<T>` - Repositorio con aislamiento multi-tenant
  - `IUnitOfWork` - Coordinacion de transacciones entre repositorios
  - `GenericRepository` y `TenantRepository` implementaciones
  - `UnitOfWork` con todos los repositorios (Core, Fleet, Warehouse, Shipment, Network)

- **DTOs Comunes**:

  - `PagedRequest` - Paginacion generica con ordenamiento y busqueda
  - `PagedResult<T>` - Respuesta paginada con metadata (TotalPages, HasNext, etc.)
  - `BaseDto`, `TenantDto` - DTOs base con campos de auditoria
  - `OperationResult`, `OperationResult<T>` - Respuestas estandarizadas

- **Infraestructura Docker**:

  - `docker-compose.yml` actualizado con servicios: postgres, api, admin, operaciones, campo, tunnel
  - PostgreSQL 17 con volumen externo `postgres_pgdata`
  - Healthchecks configurados para todos los servicios
  - Cloudflare Tunnel para acceso remoto

- **xUnit Tests (28 tests)**:

  - `PaginationDtoTests` - Validacion de paginacion
  - `GenericRepositoryTests` - CRUD, soft delete, queries
  - `InMemoryDbFixture` - Fixture con datos de prueba
  - `TestDataBuilder` - Builder pattern para entidades de test

### Modificado

- CI/CD actualizado a v0.5.1 con paso explicito `Run xUnit Tests`
- PostgreSQL actualizado a version 17 en CI
- Swagger UI habilitado en todos los entornos (desarrollo via Tailscale)

### Notas Tecnicas

- Herramientas de desarrollo local (control-panel) excluidas del repositorio
- Foundation layer es prerequisito para Services layer (v0.5.2+)
- Tests de logica de negocio se implementan en fases 3-8

---

## [0.5.0] - 2025-12-15

### Agregado

- **Endpoints API Skeleton (22 endpoints)**:

  - Core Layer: Tenants, Users, Roles, Employees, Clients
  - Warehouse Layer: Locations, WarehouseZones, WarehouseOperators, InventoryStocks, InventoryTransactions
  - Fleet Layer: Trucks, Drivers, Shifts, FleetLogs
  - Shipment Layer: Shipments, ShipmentItems, ShipmentCheckpoints, ShipmentDocuments, CatalogItems
  - Network Layer: NetworkLinks, RouteBlueprints, RouteSteps

- **Schema Metadata Endpoint**:

  - `GET /api/Schema/metadata` - Retorna estructura de BD para herramientas
  - `POST /api/Schema/refresh` - Invalida cache de metadata

- **Documentacion**:
  - Nuevo archivo `api-architecture.md` con estructura de capas y endpoints
  - Documentacion de Swagger UI en `/swagger`

### Modificado

- Version del sistema actualizada a 0.5.0
- CI/CD actualizado para verificar 24 tablas en base de datos

### Notas Tecnicas

- Endpoints responden con HTTP 200 (lista vacia) para GET autenticados
- Logica CRUD pendiente para v0.5.x
- Herramientas de desarrollo local excluidas del repositorio

---

## [0.4.4] - 2025-12-14

### Agregado

- **Catalogo Maestro de Productos (`CatalogItem`)**:

  - SKU unico por tenant con indice unico
  - Dimensiones por defecto (peso, ancho, alto, largo)
  - Flags: RequiresRefrigeration, IsHazardous, IsFragile
  - Unidad de medida base (Pza, Kg, Lt, Caja)

- **Inventario Cuantificado (`InventoryStock`)**:

  - Stock por zona de bodega con FK a `WarehouseZone`
  - Numero de lote (`BatchNumber`) y fecha de caducidad
  - Cantidad reservada y disponible
  - Costo unitario para valuacion
  - Indice unico compuesto: `(ZoneId, ProductId, BatchNumber)`
  - Indice filtrado para productos proximos a caducar

- **Kardex de Movimientos (`InventoryTransaction`)**:

  - Bitacora de todos los movimientos internos
  - Tipos: Receipt, PutAway, InternalMove, Picking, Packing, Dispatch, Adjustment, Scrap, Return
  - FK a zonas origen/destino, usuario ejecutor, envio relacionado
  - Indices para consultas por producto y fecha

- **Automatizacion de Auditoria (`AuditSaveChangesInterceptor`)**:

  - Llena automaticamente `CreatedByUserId` en inserts
  - Llena automaticamente `LastModifiedByUserId` en updates
  - Maneja `DeletedAt` en soft deletes
  - Servicios: `ICurrentUserService`, `CurrentUserService`

- **Campos de Auditoria en `BaseEntity`**:

  - `CreatedByUserId` (Guid?) - Usuario que creo el registro
  - `LastModifiedByUserId` (Guid?) - Ultimo usuario que modifico
  - `RowVersion` (uint) - Token de concurrencia optimista

- **Geolocalizacion**:

  - `Latitude` y `Longitude` (decimal, precision 9,6) en `Location`
  - `Latitude` y `Longitude` en `ShipmentCheckpoint`

- **FK `ProductId` en `ShipmentItem`**:
  - Referencia opcional a `CatalogItem`
  - Campos descriptivos se mantienen para override

### Modificado

- `BaseEntity`: +RowVersion, +CreatedByUserId, +LastModifiedByUserId
- `ShipmentItem`: +ProductId FK a CatalogItem
- `WarehouseZone`: +InventoryStocks, +OriginTransactions, +DestinationTransactions
- `Location`: +Latitude, +Longitude
- `ShipmentCheckpoint`: +Latitude, +Longitude, CreatedByUserId marcado con `new`
- `FleetLog`: CreatedByUserId marcado con `new`
- `ParhelionDbContext`: +CatalogItems, +InventoryStocks, +InventoryTransactions DbSets
- `Program.cs`: +AuditSaveChangesInterceptor, version 0.4.4

### Configuraciones EF Core

- `CatalogItemConfiguration`: SKU unico por tenant, precision de decimales
- `InventoryStockConfiguration`: Concurrencia xmin, indices filtrados
- `InventoryTransactionConfiguration`: Relaciones con zonas, usuario, envio

### Migracion

- `WmsEnhancement044` aplicada a PostgreSQL
- 3 nuevas tablas: CatalogItems, InventoryStocks, InventoryTransactions
- Total: 23 tablas en base de datos

### Tests

- 8 tests de integracion pasando
- Compatibilidad verificada con esquema anterior

---

## [0.4.3] - 2025-12-13

### Agregado

- **Employee Layer (Centralización de Datos de Empleado)**:

  - Nueva entidad `Employee` con datos legales (RFC, NSS, CURP)
  - Contacto de emergencia, fecha de contratación, departamento
  - Relación 1:1 con `User` (usuario del sistema)

- **Sistema de Turnos (`Shift`)**:

  - Nuevo registro de turnos de trabajo por tenant
  - Campos: StartTime, EndTime, DaysOfWeek
  - Asignación opcional a empleados

- **Zonas de Bodega (`WarehouseZone`)**:

  - Divisiones internas de ubicaciones (Receiving, Storage, ColdChain, etc.)
  - Enum `WarehouseZoneType` con 6 tipos de zona
  - Asignación a operadores de almacén

- **Extensión WarehouseOperator**:

  - Similar a Driver pero para almacenistas
  - Ubicación asignada, zona primaria
  - FK en `ShipmentCheckpoint.HandledByWarehouseOperatorId`

- **Super Admin (IsSuperAdmin)**:

  - Flag en `User` para administradores del sistema
  - Correo format: `nombre@parhelion.com`
  - Nuevo rol `SystemAdmin` en SeedData

- **20 Nuevos Permisos**:

  - Employees: Read, Create, Update, Delete
  - Shifts: Read, Create, Update, Delete
  - WarehouseZones: Read, Create, Update, Delete
  - WarehouseOperators: Read, Create, Update, Delete
  - Tenants: Read, Create, Update, Deactivate

### Modificado

- `Driver`: Refactorizado de `TenantEntity` a `BaseEntity`
  - `UserId` → `EmployeeId` (datos legales movidos a Employee)
- `User`: Agregado `IsSuperAdmin`, `Employee` navigation
- `Location`: Agregado `Zones` y `AssignedWarehouseOperators`
- `ShipmentCheckpoint`: Agregado `HandledByWarehouseOperatorId`

### Tests

- 7 tests de integración E2E para Employee Layer
- Cobertura: Tenant, User, Employee, Driver, WarehouseOperator, Shift, Checkpoint

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
