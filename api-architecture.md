# Arquitectura de API - Parhelion Logistics

Documentacion tecnica de la estructura API-First del backend Parhelion.

## Estado Actual

**Version:** 0.6.0-alpha
**Enfoque:** Python Microservice Integration + Analytics Foundation
**Arquitectura:** Clean Architecture + Domain-Driven Design + Microservices

---

## Capas del API (API Layers)

El backend esta organizado en 5 capas logicas que agrupan los endpoints segun su dominio:

### Core Layer

Gestion de identidad, usuarios y estructura organizacional.

| Endpoint         | Entidad  | Estado   | Service         |
| ---------------- | -------- | -------- | --------------- |
| `/api/tenants`   | Tenant   | Services | TenantService   |
| `/api/users`     | User     | Services | UserService     |
| `/api/roles`     | Role     | Services | RoleService     |
| `/api/employees` | Employee | Services | EmployeeService |
| `/api/clients`   | Client   | Services | ClientService   |

### Warehouse Layer

Gestion de almacenes, zonas e inventario.

| Endpoint                      | Entidad              | Estado   | Service                     |
| ----------------------------- | -------------------- | -------- | --------------------------- |
| `/api/locations`              | Location             | Services | LocationService             |
| `/api/warehouse-zones`        | WarehouseZone        | Services | WarehouseZoneService        |
| `/api/warehouse-operators`    | WarehouseOperator    | Services | WarehouseOperatorService    |
| `/api/inventory-stocks`       | InventoryStock       | Services | InventoryStockService       |
| `/api/inventory-transactions` | InventoryTransaction | Services | InventoryTransactionService |

### Fleet Layer

Gestion de flotilla, choferes y turnos.

| Endpoint          | Entidad  | Estado   | Service         |
| ----------------- | -------- | -------- | --------------- |
| `/api/trucks`     | Truck    | Services | TruckService    |
| `/api/drivers`    | Driver   | Services | DriverService   |
| `/api/shifts`     | Shift    | Skeleton | -               |
| `/api/fleet-logs` | FleetLog | Services | FleetLogService |

### Shipment Layer

Gestion de envios, items y trazabilidad.

| Endpoint                    | Entidad            | Estado   | Service                   |
| --------------------------- | ------------------ | -------- | ------------------------- | -------------------------------------- |
| `/api/shipments`            | Shipment           | Services | ShipmentService           |
| `/api/shipment-items`       | ShipmentItem       | Services | ShipmentItemService       |
| `/api/shipment-checkpoints` | ShipmentCheckpoint | Services | ShipmentCheckpointService | `timeline/{id}`, `/by-status`, `/last` |
| `/api/shipment-documents`   | ShipmentDocument   | Services | ShipmentDocumentService   | `/pod/{id}`                            |
| `/api/documents`            | -                  | Services | PdfGeneratorService       | PDF Generation (v0.5.7)                |
| `/api/catalog-items`        | CatalogItem        | Services | CatalogItemService        |                                        |
| `/api/notifications`        | Notification       | Services | NotificationService       |                                        |

### Documents Layer (v0.5.7 NEW)

Generación dinámica de documentos PDF sin almacenamiento.

| Endpoint                                | Documento           | Entidad Input  |
| --------------------------------------- | ------------------- | -------------- |
| `GET /api/documents/service-order/{id}` | Orden de Servicio   | Shipment       |
| `GET /api/documents/waybill/{id}`       | Carta Porte         | Shipment       |
| `GET /api/documents/manifest/{id}`      | Manifiesto de Carga | RouteBlueprint |
| `GET /api/documents/trip-sheet/{id}`    | Hoja de Ruta        | Driver         |
| `GET /api/documents/pod/{id}`           | Proof of Delivery   | Shipment       |

> Los PDFs se generan on-demand con datos de BD. Cliente crea `blob:` URL local.

### Network Layer

Gestion de red logistica Hub and Spoke.

| Endpoint                | Entidad        | Estado   | Service            |
| ----------------------- | -------------- | -------- | ------------------ |
| `/api/network-links`    | NetworkLink    | Services | NetworkLinkService |
| `/api/route-blueprints` | RouteBlueprint | Services | RouteService       |
| `/api/route-steps`      | RouteStep      | Services | RouteStepService   |

---

## Services Layer (v0.5.2)

Capa de servicios que encapsula logica de negocio.

### Interfaces Base

| Interfaz             | Descripcion                               |
| -------------------- | ----------------------------------------- |
| `IGenericService<T>` | CRUD generico con paginacion y DTOs       |
| `ITenantService`     | Extiende IGenericService para Tenants     |
| `IUserService`       | Validacion de credenciales, cambio passwd |
| `IShipmentService`   | Tracking, asignacion, estatus             |

### Implementaciones por Capa

| Capa      | Services                                                               |
| --------- | ---------------------------------------------------------------------- |
| Core      | Tenant, User, Role, Employee, Client                                   |
| Shipment  | Shipment, ShipmentItem, Checkpoint, Document, Catalog                  |
| Fleet     | Driver, Truck, FleetLog                                                |
| Network   | Location, Route, NetworkLink, RouteStep                                |
| Warehouse | WarehouseZone, WarehouseOperator, InventoryStock, InventoryTransaction |

---

## Foundation Layer (v0.5.1)

Infraestructura base para operaciones CRUD y transacciones.

### Repository Pattern

| Interfaz                | Implementacion      | Descripcion                      |
| ----------------------- | ------------------- | -------------------------------- |
| `IGenericRepository<T>` | `GenericRepository` | CRUD generico con soft delete    |
| `ITenantRepository<T>`  | `TenantRepository`  | Filtrado automatico por TenantId |
| `IUnitOfWork`           | `UnitOfWork`        | Coordinacion de transacciones    |

---

## Autenticacion

Todos los endpoints protegidos requieren JWT Bearer token:

```http
Authorization: Bearer <access_token>
```

El token se obtiene via `/api/auth/login` con credenciales validas.

---

## Health Endpoints

| Endpoint         | Descripcion                   |
| ---------------- | ----------------------------- |
| `GET /health`    | Estado del servicio           |
| `GET /health/db` | Conectividad de base de datos |

---

## Base de Datos

- **Tablas:** 24
- **Migraciones:** Aplicadas (EF Core Code First)
- **Provider:** PostgreSQL 17

---

## Tests (xUnit)

| Test Suite               | Tests   | Cobertura                  |
| ------------------------ | ------- | -------------------------- |
| `PaginationDtoTests`     | 11      | PagedRequest, PagedResult  |
| `GenericRepositoryTests` | 9       | CRUD, Soft Delete, Queries |
| `ServiceTests`           | 72      | All Services               |
| `BusinessRulesTests`     | 30      | Compatibility, FleetLog    |
| **Total**                | **122** | Full backend coverage      |

---

## Python Analytics Service (v0.6.0+)

Microservicio local para análisis avanzado, predicciones y reportes.

### Tecnologías

| Componente | Tecnología               |
| ---------- | ------------------------ |
| Framework  | FastAPI 0.115+           |
| Runtime    | Python 3.12+             |
| ORM        | SQLAlchemy 2.0 + asyncpg |
| Validación | Pydantic v2              |
| Testing    | pytest + pytest-asyncio  |

### Endpoints Python

| Endpoint                      | Método | Descripción                      |
| ----------------------------- | ------ | -------------------------------- |
| `/health`                     | GET    | Estado del servicio              |
| `/health/db`                  | GET    | Conectividad PostgreSQL          |
| `/api/py/analytics/shipments` | GET    | Análisis de envíos por período   |
| `/api/py/analytics/fleet`     | GET    | Métricas de utilización de flota |
| `/api/py/predictions/eta`     | POST   | Predicción de ETA con ML         |
| `/api/py/reports/export`      | POST   | Generación de reportes Excel     |

### Autenticación Python

Requiere header `X-Internal-Service-Key` para llamadas desde .NET API,
o `Authorization: Bearer <jwt>` para llamadas desde n8n.

### Comunicación Inter-Servicios

```mermaid
flowchart LR
    subgraph Docker["Docker Network"]
        NET[".NET API<br/>:5000"]
        PY["Python Analytics<br/>:8000"]
        DB[(PostgreSQL<br/>:5432)]
    end

    NET <-->|"REST/JSON<br/>Internal JWT"| PY
    NET --> DB
    PY --> DB
```

---

## Pendientes (v0.7.0+)

Los siguientes items quedan pendientes para futuras versiones:

- [ ] QR Handshake (Transferencia de custodia digital via QR)
- [ ] Route Assignment (Asignación de rutas a shipments)
- [ ] Dashboard (KPIs operativos con procesamiento Python)
- [ ] Predicción ETA con ML (Python)
- [ ] Exportación Excel dinámica (Python + pandas)
- [ ] Recuperación de contraseña
- [ ] Demo Mode para reclutadores

---

## Notas de Desarrollo

La gestion de endpoints durante desarrollo utiliza herramientas privadas que no forman parte del repositorio. Estas herramientas contienen credenciales y configuraciones sensibles que no deben exponerse publicamente.

---

**Ultima actualizacion:** 2025-12-28
