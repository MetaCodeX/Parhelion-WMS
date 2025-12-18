# Arquitectura de API - Parhelion Logistics

Documentacion tecnica de la estructura API-First del backend Parhelion.

## Estado Actual

**Version:** 0.5.4  
**Enfoque:** Swagger/OpenAPI + Business Logic  
**Arquitectura:** Clean Architecture + Domain-Driven Design

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

| Endpoint                      | Entidad              | Estado   | Service         |
| ----------------------------- | -------------------- | -------- | --------------- |
| `/api/locations`              | Location             | Services | LocationService |
| `/api/warehouse-zones`        | WarehouseZone        | Skeleton | -               |
| `/api/warehouse-operators`    | WarehouseOperator    | Skeleton | -               |
| `/api/inventory-stocks`       | InventoryStock       | Skeleton | -               |
| `/api/inventory-transactions` | InventoryTransaction | Skeleton | -               |

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
| --------------------------- | ------------------ | -------- | ------------------------- |
| `/api/shipments`            | Shipment           | Services | ShipmentService           |
| `/api/shipment-items`       | ShipmentItem       | Services | ShipmentItemService       |
| `/api/shipment-checkpoints` | ShipmentCheckpoint | Services | ShipmentCheckpointService |
| `/api/shipment-documents`   | ShipmentDocument   | Services | ShipmentDocumentService   |
| `/api/catalog-items`        | CatalogItem        | Services | CatalogItemService        |

### Network Layer

Gestion de red logistica Hub and Spoke.

| Endpoint                | Entidad        | Estado   | Service      |
| ----------------------- | -------------- | -------- | ------------ |
| `/api/network-links`    | NetworkLink    | Skeleton | -            |
| `/api/route-blueprints` | RouteBlueprint | Services | RouteService |
| `/api/route-steps`      | RouteStep      | Skeleton | -            |

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

| Capa     | Services                                              |
| -------- | ----------------------------------------------------- |
| Core     | Tenant, User, Role, Employee, Client                  |
| Shipment | Shipment, ShipmentItem, Checkpoint, Document, Catalog |
| Fleet    | Driver, Truck, FleetLog                               |
| Network  | Location, Route                                       |

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

| Test Suite               | Tests  | Cobertura                  |
| ------------------------ | ------ | -------------------------- |
| `PaginationDtoTests`     | 11     | PagedRequest, PagedResult  |
| `GenericRepositoryTests` | 9      | CRUD, Soft Delete, Queries |
| **Total**                | **28** | Foundation layer           |

---

## Pendientes (v0.5.2+)

Los siguientes items quedan pendientes para futuras versiones:

- [ ] Servicios CRUD por entidad (TenantService, ShipmentService, etc.)
- [ ] Validaciones de DTOs con FluentValidation
- [ ] Calculos de peso volumetrico y costos
- [ ] Reglas de negocio (compatibilidad de carga, cadena de frio)
- [ ] Generacion de documentos legales (Carta Porte, POD)
- [ ] Tests de logica de negocio (Fases 3-8)

---

## Notas de Desarrollo

La gestion de endpoints durante desarrollo utiliza herramientas privadas que no forman parte del repositorio. Estas herramientas contienen credenciales y configuraciones sensibles que no deben exponerse publicamente.

---

**Ultima actualizacion:** 2025-12-16
