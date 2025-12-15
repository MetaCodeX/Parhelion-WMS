# Arquitectura de API - Parhelion Logistics

Documentacion tecnica de la estructura API-First del backend Parhelion.

## Estado Actual

**Version:** 0.5.0  
**Enfoque:** API-First (Skeleton Endpoints)  
**Arquitectura:** Clean Architecture + Domain-Driven Design

---

## Capas del API (API Layers)

El backend esta organizado en 5 capas logicas que agrupan los endpoints segun su dominio:

### Core Layer

Gestion de identidad, usuarios y estructura organizacional.

| Endpoint         | Entidad  | Estado   |
| ---------------- | -------- | -------- |
| `/api/tenants`   | Tenant   | Skeleton |
| `/api/users`     | User     | Skeleton |
| `/api/roles`     | Role     | Skeleton |
| `/api/employees` | Employee | Skeleton |
| `/api/clients`   | Client   | Skeleton |

### Warehouse Layer

Gestion de almacenes, zonas e inventario.

| Endpoint                      | Entidad              | Estado   |
| ----------------------------- | -------------------- | -------- |
| `/api/locations`              | Location             | Skeleton |
| `/api/warehouse-zones`        | WarehouseZone        | Skeleton |
| `/api/warehouse-operators`    | WarehouseOperator    | Skeleton |
| `/api/inventory-stocks`       | InventoryStock       | Skeleton |
| `/api/inventory-transactions` | InventoryTransaction | Skeleton |

### Fleet Layer

Gestion de flotilla, choferes y turnos.

| Endpoint          | Entidad  | Estado   |
| ----------------- | -------- | -------- |
| `/api/trucks`     | Truck    | Skeleton |
| `/api/drivers`    | Driver   | Skeleton |
| `/api/shifts`     | Shift    | Skeleton |
| `/api/fleet-logs` | FleetLog | Skeleton |

### Shipment Layer

Gestion de envios, items y trazabilidad.

| Endpoint                    | Entidad            | Estado   |
| --------------------------- | ------------------ | -------- |
| `/api/shipments`            | Shipment           | Skeleton |
| `/api/shipment-items`       | ShipmentItem       | Skeleton |
| `/api/shipment-checkpoints` | ShipmentCheckpoint | Skeleton |
| `/api/shipment-documents`   | ShipmentDocument   | Skeleton |
| `/api/catalog-items`        | CatalogItem        | Skeleton |

### Network Layer

Gestion de red logistica Hub and Spoke.

| Endpoint                | Entidad        | Estado   |
| ----------------------- | -------------- | -------- |
| `/api/network-links`    | NetworkLink    | Skeleton |
| `/api/route-blueprints` | RouteBlueprint | Skeleton |
| `/api/route-steps`      | RouteStep      | Skeleton |

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

## Documentacion Interactiva

Swagger UI disponible en entorno de desarrollo:

```
http://localhost:5100/swagger
```

---

## Base de Datos

- **Tablas:** 24
- **Migraciones:** Aplicadas (EF Core Code First)
- **Provider:** PostgreSQL 17

---

## Pendientes

Los siguientes items quedan pendientes para futuras versiones:

- Implementacion de logica CRUD completa en cada endpoint
- Validaciones de DTOs con FluentValidation
- Calculos de peso volumetrico y costos
- Reglas de negocio (compatibilidad de carga, cadena de frio)
- Generacion de documentos legales (Carta Porte, POD)
- Tests unitarios y de integracion por endpoint

---

## Notas de Desarrollo

La gestion de endpoints durante desarrollo utiliza herramientas privadas que no forman parte del repositorio. Estas herramientas contienen credenciales y configuraciones sensibles que no deben exponerse publicamente.

---

**Ultima actualizacion:** 2025-12-15
