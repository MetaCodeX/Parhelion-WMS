# Changelog

Historial de cambios del proyecto Parhelion Logistics.

---

## [0.6.0-alpha] - 2025-12-28 (En Progreso)

### Nuevo Sistema de Versionado

A partir de esta versión, el proyecto adopta **Semantic Versioning (SemVer)** estricto con pre-releases:

```
MAJOR.MINOR.PATCH-PRERELEASE+BUILD
Ejemplo: 0.6.0-alpha.1+build.2025.12.28
```

| Etapa   | Significado                                 |
| ------- | ------------------------------------------- |
| `alpha` | Desarrollo activo, funcionalidad incompleta |
| `beta`  | Feature-complete, en testing                |
| `rc`    | Release Candidate, listo para producción    |

### Agregado

- **Python Analytics Service** (Microservicio local):

  - Framework: FastAPI 0.115+ con Python 3.12
  - Arquitectura: Clean Architecture (domain, application, infrastructure, api)
  - ORM: SQLAlchemy 2.0 + asyncpg (async PostgreSQL)
  - Bounded Context: Analytics & Predictions (separado del Core .NET)
  - Puerto interno: 8000
  - Container name: `parhelion-python`

- **Preparativos de Integración**:

  - Documentación actualizada: README.md, api-architecture.md, database-schema.md
  - `.gitignore` con patrones Python (**pycache**, .venv, .pytest_cache, etc.)
  - Nuevo roadmap hacia v1.0.0 MVP (Q1 2026)
  - Sistema de versionado SemVer con staged releases (alpha → beta → rc)

### Modificado

- `docker-compose.yml` - Preparado para servicio `python-analytics`
- `README.md` - Stack tecnológico expandido con Python/FastAPI
- `.github/workflows/ci.yml` - Estructura preparada para job Python

### Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                    Docker Network                            │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────────────┐ │
│  │ .NET API│◄─┤PostgreSQL├─►│ Python  │  │      n8n        │ │
│  │  :5000  │  │  :5432  │  │  :8000  │  │     :5678       │ │
│  └────┬────┘  └─────────┘  └────┬────┘  └────────┬────────┘ │
│       │                         │                 │          │
│       └─────────────────────────┴─────────────────┘          │
│                    Internal REST/JSON                        │
└─────────────────────────────────────────────────────────────┘
```

### Notas de Migración

- Nueva variable de entorno requerida: `INTERNAL_SERVICE_KEY` para auth inter-servicios
- Volume nuevo: `python_cache` para modelos ML (futuro)
- El microservicio Python es local (como n8n), no expuesto públicamente

---

## [0.6.0-beta] - 2025-12-29

### Refactorización de Autorización Multi-Tenant

- **UserService.CreateAsync refactorizado:**

  - SuperAdmin puede especificar `targetTenantId` para crear Admin de otro Tenant
  - Tenant Admin crea usuarios que heredan `TenantId` automáticamente
  - Validación: Non-SuperAdmin no puede usar `targetTenantId`

- **AuditSaveChangesInterceptor mejorado:**

  - Asignación automática de `TenantId` para todas las `TenantEntity`
  - TenantId extraído del contexto del usuario creador

- **CreateUserRequest DTO actualizado:**
  - Nuevo campo opcional: `TargetTenantId` (solo para SuperAdmin)

### Base de Datos

- **Reset completo de datos de prueba:**

  - Sistema limpio con flujo multi-tenant correcto
  - 1 SuperAdmin (MetaCodeX CEO)
  - 1 Tenant ejemplo (TransporteMX) con recursos completos

- **Script de reset:** `scripts/reset_database.sql`

### Nuevo Tenant de Prueba: TransporteMX

| Entidad   | Cantidad | Detalles                          |
| --------- | -------- | --------------------------------- |
| Users     | 6        | 1 Admin + 3 Drivers + 2 Warehouse |
| Employees | 5        | Vinculados a Users                |
| Drivers   | 3        | Con Trucks asignados              |
| Trucks    | 3        | DryBox, Refrigerated, Flatbed     |
| Locations | 4        | 3 Hubs + 1 Store                  |

### Documentación

- `api-architecture.md` - Nueva sección "Autenticación y Autorización" con diagrama Mermaid
- `README.md` - Actualizado con Multi-tenancy Automático
- `credentials.dev.txt` - Archivo de credenciales de desarrollo con todos los IDs

### Preparación para Stress Tests

- `scripts/stress_tests.py` - Actualizado con credenciales y IDs correctos
- 5 tests de estrés listos para ejecución:
  1. Generación masiva (500 shipments)
  2. Fragmentación de red
  3. Simulación de caos operativo
  4. Concurrencia Polly
  5. Optimización de carga 3D

---

## [0.5.7] - 2025-12-23

### Agregado

- **Frontend Landing Page (frontend-inicio)**:

  - Nuevo proyecto Angular 18 con diseño Neo-Brutalism
  - 10 componentes animados: Marquee, Buttons, Badges, Cards, Tabs, Progress Bars, Carousel, Accordion, Alert, Grid Animation
  - Carousel con changelog completo (8 slides: v0.1.0 → v0.5.7)
  - Tabs con características: Core, Flotilla, Documentos, Automatización
  - Diseño responsive mobile-first (5 breakpoints: 320px, 480px, 768px, 1024px, 1280px+)
  - Enlaces a Panel Admin, Operaciones y Driver App
  - Accesibilidad: Touch device enhancements, Reduced motion support

- **Infraestructura Cloudflare Tunnel**:

  - Subdominios públicos configurados:
    - `parhelion.macrostasis.lat` → Landing Page
    - `phadmin.macrostasis.lat` → Panel Admin
    - `phops.macrostasis.lat` → Operaciones (PWA)
    - `phdriver.macrostasis.lat` → Driver App (PWA)
    - `phapi.macrostasis.lat` → Backend API
  - Docker service `inicio` agregado a `docker-compose.yml`
  - Nginx + Multi-stage build para producción

- **Generación Dinámica de PDFs** (Sin almacenamiento de archivos):

  - `IPdfGeneratorService` - Interface para generación on-demand de documentos
  - `PdfGeneratorService` - Implementación con plantillas HTML para 5 tipos de documentos
  - `DocumentsController` - Nuevo controller con endpoints protegidos por JWT:
    - `GET /api/documents/service-order/{shipmentId}` - Orden de Servicio
    - `GET /api/documents/waybill/{shipmentId}` - Carta Porte
    - `GET /api/documents/manifest/{routeId}` - Manifiesto de Carga
    - `GET /api/documents/trip-sheet/{driverId}` - Hoja de Ruta
    - `GET /api/documents/pod/{shipmentId}` - Prueba de Entrega (POD)
  - Los PDFs se generan en memoria usando datos de BD + plantilla
  - Cliente crea `blob:` URL local (estilo WhatsApp Web)

- **Proof of Delivery (POD) con Firma Digital**:

  - Nuevos campos en `ShipmentDocument`: `SignatureBase64`, `SignedByName`, `SignedAt`, `SignatureLatitude`, `SignatureLongitude`
  - Endpoint `POST /api/shipment-documents/pod/{shipmentId}` para captura de firma
  - Campos de metadata: `OriginalFileName`, `ContentType`, `FileSizeBytes`
  - Migración `AddPodSignatureFields` aplicada

- **Timeline de Checkpoints (Visualización Metro)**:

  - `CheckpointTimelineItem` DTO con StatusLabel en español
  - `IShipmentCheckpointService.GetTimelineAsync()` con datos simplificados
  - Endpoint `GET /api/shipment-checkpoints/timeline/{shipmentId}`
  - Labels: Cargado, QR escaneado, Llegó a Hub, En camino, Entregado, etc.

### Modificado

- **Refactorización de Controllers a Clean Architecture**:

  - `ShipmentCheckpointsController` - Ahora usa `IShipmentCheckpointService` (antes: DbContext directo)
  - `ShipmentDocumentsController` - Simplificado, delegación a servicios
  - Agregados endpoints de filtrado: `/by-status`, `/last`, `/timeline`

- `ShipmentCheckpointService` - Inyección de `IWebhookPublisher` y `ILogger`
- `ShipmentCheckpointService.CreateAsync` - Publica webhook `checkpoint.created` tras guardar
- `Program.cs` - Registro de `IPdfGeneratorService`, versión `0.5.7`
- `Dockerfile` - Actualizado a version 0.5.7
- `docker-compose.yml` - Agregado servicio `inicio` en puerto 4000

### Eliminado

- `LocalFileStorageService` - Ya no se almacenan archivos permanentemente
- `IFileStorageService` - Reemplazado por generación dinámica
- Test scripts temporales (`test_e2e_full.sh`, `test_v057.sh`)

---

## [0.5.6] - 2025-12-22

### Agregado

- **Sistema de Webhooks (Backend → n8n)**:

  - `IWebhookPublisher` - Interface en Application Layer
  - `N8nWebhookPublisher` - Implementación fire-and-forget con logging
  - `ICallbackTokenService` & `CallbackTokenService` - Implementación de tokens JWT efímeros (15m)
  - `NullWebhookPublisher` - Implementación vacía para desactivar webhooks
  - `N8nConfiguration` - Configuración tipada desde appsettings
  - 5 DTOs de eventos: ShipmentException, BookingRequest, HandshakeAttempt, StatusChanged, CheckpointCreated

- **Sistema de Notificaciones (n8n → Backend)**:

  - Nueva entidad `Notification` con tipos (Alert, Info, Warning, Success) y prioridades
  - `NotificationsController` con endpoints para n8n (POST) y apps móviles (GET)
  - `INotificationService` + `NotificationService` implementación
  - Migración `AddNotifications` aplicada

- **Autenticación de Servicios Externos (Multi-Tenant)**:

  - Nueva entidad `ServiceApiKey` con TenantId, KeyHash (SHA256), Scopes, Expiración
  - `ServiceApiKeyAttribute` - Filtro que valida X-Service-Key contra BD
  - Lookup de TenantId desde tabla en lugar de hardcoding
  - **Generación automática de API Key** al crear nuevo Tenant (responsabilidad del SuperAdmin)
  - Migración `AddServiceApiKeys` aplicada

- **Telemetría GPS de Camiones**:

  - Campos `LastLatitude`, `LastLongitude`, `LastLocationUpdate` en Truck
  - Endpoint `POST /api/trucks/{id}/location` para simulación GPS

- **Búsqueda Geoespacial de Choferes**:

  - `IDriverService.GetNearbyDriversAsync` con fórmula Haversine
  - Endpoint `GET /api/drivers/nearby?lat=&lon=&radius=`
  - Filtrado por DriverStatus.Available y TenantId

### Modificado

- `DriversController.GetNearby` - Ahora resuelve TenantId desde ServiceApiKey (producción-ready)
- `ServiceApiKeyAttribute` - Refinado para soportar auth híbrida (Header `X-Service-Key` y `Authorization: Bearer`)
- `ShipmentService.UpdateStatusAsync` - Publica webhook `shipment.exception` automáticamente
- `Program.cs` - Registro condicional de IWebhookPublisher (N8n o Null)
- `docker-compose.yml` - Agregado servicio n8n con PostgreSQL compartido
- Estandarización de "Envelope" JSON para todos los eventos de sistema (CorrelationId, Timestamp, CallbackToken)

### Seguridad

- API Keys almacenadas como SHA256 hash (nunca plain text)
- Validación de expiración y estado activo
- Rate limiting de actualización LastUsedAt (fire-and-forget)

### Notas Técnicas

- Webhooks son fire-and-forget: errores se loguean pero no interrumpen flujo
- Configuración `N8n:Enabled` controla activación de webhooks
- ServiceApiKeys requieren seed manual para tenants

---

## [0.5.5] - 2025-12-18

### Agregado

- **WMS Services Layer (4 servicios)**:

  - `WarehouseZoneService` - Gestión de zonas de almacén
  - `WarehouseOperatorService` - Gestión de almacenistas
  - `InventoryStockService` - Stock con reserva/liberación
  - `InventoryTransactionService` - Kardex de movimientos (Receipt, Dispatch, Transfer)

- **TMS Network Services (2 servicios)**:

  - `NetworkLinkService` - Enlaces bidireccionales entre nodos
  - `RouteStepService` - Pasos de ruta con reordenamiento automático

- **Business Rules Validators**:

  - `ICargoCompatibilityValidator` - Interface de validación carga-camión
  - `CargoCompatibilityValidator` - Implementación con reglas:
    - Cargo refrigerado → Truck Refrigerated
    - Cargo HAZMAT → Truck HazmatTank
    - Cargo alto valor (>$500K) → Truck Armored

- **Automatic FleetLog Generation**:

  - `DriverService.AssignTruckAsync` ahora genera FleetLog automáticamente
  - Audit trail completo de cambios de camión

- **Airport Code Validation**:

  - `LocationService.CreateAsync` valida formato 2-4 letras (MTY, GDL, MM)
  - Normalización automática a mayúsculas

- **Tests (50 nuevos, total: 122)**:

  - Unit tests para WMS services (15)
  - Unit tests para Network services (10)
  - Integration tests WMS/Fleet/Network (13)
  - CargoCompatibilityValidator tests (12)

### Modificado

- `ShipmentService.AssignToDriverAsync` - Ahora valida compatibilidad carga-camión
- `Program.cs` - Registro de todos los nuevos servicios en DI
- `ServiceTestFixture` - Datos seed para WMS (Zone, CatalogItem, InventoryStock)

### Notas Técnicas

- Validators registrados como Singleton (stateless)
- Services registrados como Scoped
- Todas las validaciones son fail-safe con mensajes descriptivos

---

## [0.5.4] - 2025-12-18

### Agregado

- **Swagger/OpenAPI Documentation**:

  - OpenAPI Info con version, titulo, descripcion, contacto
  - JWT Bearer Security Scheme con autorizacion
  - XML Comments habilitados para documentacion automatica
  - Atributos `[Produces]` y `[Consumes]` en Controllers

- **Business Logic - Shipment Workflow**:
  - Validación de transiciones de estado (`ValidateStatusTransition`)
  - Workflow: PendingApproval → Approved → Loaded → InTransit → AtHub/OutForDelivery → Delivered
  - Estado Exception para manejo de problemas con recuperación

### Modificado

- **Controllers Refactorizados (5 total)**:

  - `TrucksController` → `ITruckService`
  - `DriversController` → `IDriverService`
  - `FleetLogsController` → `IFleetLogService`
  - `LocationsController` → `ILocationService`
  - `RouteBlueprintsController` → `IRouteService`

- **Nuevos Endpoints**:
  - `PATCH /api/drivers/{id}/assign-truck` - Asignar camión a chofer
  - `PATCH /api/drivers/{id}/status` - Actualizar estatus de chofer
  - `POST /api/fleet-logs/start-usage` - Iniciar uso de camión
  - `POST /api/fleet-logs/end-usage` - Finalizar uso de camión
  - `GET /api/route-blueprints/{id}/steps` - Obtener pasos de ruta

### Notas Tecnicas

- Controllers ahora son thin wrappers que delegan a Services
- Validación de workflow previene transiciones inválidas de estado
- Preparación para tests de Business Logic en v0.5.5

---

## [0.5.3] - 2025-12-18

### Agregado

- **Integration Tests para Services Layer (44 tests nuevos)**:

  - `ServiceTestFixture` - Fixture con UnitOfWork real y datos de prueba
  - `TestIds` - IDs conocidos para testing consistente
  - **Core Services Tests**: TenantServiceTests (10), RoleServiceTests (8), EmployeeServiceTests (6), ClientServiceTests (4)
  - **Shipment Services Tests**: ShipmentServiceTests (3)
  - **Fleet Services Tests**: TruckServiceTests (8)
  - **Network Services Tests**: LocationServiceTests (5)

### Modificado

- Total de tests: 28 → 72 (incremento de 44 tests)
- Estructura de tests reorganizada en Unit/Services/{Layer}

### Notas Tecnicas

- Tests usan InMemory Database para aislamiento
- Cada test crea instancia fresca de UnitOfWork
- Cobertura de CRUD, validaciones de duplicados, y filtros

---

## [0.5.2] - 2025-12-17

### Agregado

- **Services Layer (16 interfaces, 15 implementaciones)**:

  - `IGenericService<T>` - Interfaz base con operaciones CRUD genericas
  - **Core Services**: TenantService, UserService, RoleService, EmployeeService, ClientService
  - **Shipment Services**: ShipmentService, ShipmentItemService, ShipmentCheckpointService, ShipmentDocumentService, CatalogItemService
  - **Fleet Services**: DriverService, TruckService, FleetLogService
  - **Network Services**: LocationService, RouteService

- **Refactorizacion de Controllers**:

  - TenantsController, UsersController, RolesController, EmployeesController, ClientsController, ShipmentsController
  - Controllers ahora usan Service interfaces en lugar de acceso directo a DbContext
  - Cumplimiento estricto de Clean Architecture

- **Nuevos Endpoints en ShipmentsController**:

  - `GET /api/shipments/by-tracking/{trackingNumber}` - Busqueda por tracking number
  - `GET /api/shipments/by-status/{status}` - Filtrado por estatus
  - `GET /api/shipments/by-driver/{driverId}` - Envios por chofer
  - `GET /api/shipments/by-location/{locationId}` - Envios por ubicacion
  - `PATCH /api/shipments/{id}/assign` - Asignacion de chofer y camion
  - `PATCH /api/shipments/{id}/status` - Actualizacion de estatus

### Modificado

- **Dependency Injection**: Registro de 15 Services en Program.cs organizado por capas
- **Program.cs**: Estructura clara con secciones Core, Shipment, Fleet, Network

### Notas Tecnicas

- Services Layer encapsula logica de negocio y validaciones
- Controllers reducidos a thin wrappers (delegacion a Services)
- IUnitOfWork se inyecta en Services para coordinacion de repositorios
- Preparacion para implementacion de tests de integracion en v0.5.3

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

1. ~~Implementar Domain Layer (entidades)~~
2. ~~Configurar Infrastructure Layer (EF Core)~~
3. Crear API endpoints básicos (CRUD)
4. Implementar autenticación JWT
5. Diseñar UI del Admin
6. Probar Docker con PostgreSQL
