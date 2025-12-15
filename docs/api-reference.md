# Parhelion WMS - API Reference v0.5.0

Documentación técnica de los endpoints REST API del sistema Parhelion Logistics.

## Autenticación

Todos los endpoints protegidos requieren un JWT Bearer token:

```http
Authorization: Bearer <access_token>
```

### Obtener Token

```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

---

## Endpoints por Capa

### 🔶 Core Layer (5 endpoints)

| Endpoint         | Métodos                | Descripción                            |
| ---------------- | ---------------------- | -------------------------------------- |
| `/api/tenants`   | GET, POST, PUT, DELETE | Multi-tenant management                |
| `/api/users`     | GET, POST, PUT, DELETE | User accounts                          |
| `/api/roles`     | GET, POST, PUT, DELETE | Role definitions (Admin, Driver, etc.) |
| `/api/employees` | GET, POST, PUT, DELETE | Employee profiles                      |
| `/api/clients`   | GET, POST, PUT, DELETE | B2B clients (senders/recipients)       |

### 🏭 Warehouse Layer (5 endpoints)

| Endpoint                      | Métodos                | Descripción                   |
| ----------------------------- | ---------------------- | ----------------------------- |
| `/api/locations`              | GET, POST, PUT, DELETE | Hubs, Warehouses, Cross-docks |
| `/api/warehouse-zones`        | GET, POST, PUT, DELETE | Zones within locations        |
| `/api/warehouse-operators`    | GET, POST, PUT, DELETE | Operators assigned to zones   |
| `/api/inventory-stocks`       | GET, POST, PUT, DELETE | Stock by zone/lot             |
| `/api/inventory-transactions` | GET, POST              | Kardex movements              |

### 🚛 Fleet Layer (4 endpoints)

| Endpoint          | Métodos                | Descripción                      |
| ----------------- | ---------------------- | -------------------------------- |
| `/api/trucks`     | GET, POST, PUT, DELETE | DryBox, Refrigerated, HAZMAT     |
| `/api/drivers`    | GET, POST, PUT, DELETE | Fleet drivers with MX legal data |
| `/api/shifts`     | GET, POST, PUT, DELETE | Work shifts configuration        |
| `/api/fleet-logs` | GET, POST              | Driver-Truck assignment log      |

### 📦 Shipment Layer (5 endpoints)

| Endpoint                    | Métodos                | Descripción                  |
| --------------------------- | ---------------------- | ---------------------------- |
| `/api/shipments`            | GET, POST, PUT, DELETE | Shipments PAR-XXXXXX         |
| `/api/shipment-items`       | GET, POST, PUT, DELETE | Items with volumetric weight |
| `/api/shipment-checkpoints` | GET, POST              | Immutable tracking events    |
| `/api/shipment-documents`   | GET, POST, DELETE      | B2B docs: Waybill, POD       |
| `/api/catalog-items`        | GET, POST, PUT, DELETE | Product catalog              |

### 🌐 Network Layer (3 endpoints)

| Endpoint                | Métodos                | Descripción                    |
| ----------------------- | ---------------------- | ------------------------------ |
| `/api/network-links`    | GET, POST, PUT, DELETE | FirstMile, LineHaul, LastMile  |
| `/api/route-blueprints` | GET, POST, PUT, DELETE | Predefined Hub & Spoke routes  |
| `/api/route-steps`      | GET, POST, PUT, DELETE | Route stops with transit times |

---

## Health Endpoints

```bash
GET /health        # Service status
GET /health/db     # Database connectivity
```

---

## Schema Metadata

```bash
GET /api/Schema/metadata    # Database schema for tooling
POST /api/Schema/refresh    # Force cache refresh
```

---

## Swagger UI

Documentación interactiva disponible en:

```
http://localhost:5100/swagger
```

> **Nota**: Swagger está configurado solo para entornos de desarrollo. En producción se deshabilita automáticamente.

---

## Respuestas Estándar

### Exitoso (200/201)

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "createdAt": "2025-12-14T23:00:00Z",
  ...
}
```

### Error de Autenticación (401)

```json
{
  "error": "Email o contraseña incorrectos"
}
```

### Error de Validación (400)

```json
{
  "errors": {
    "Field": ["Mensaje de error"]
  }
}
```

---

**Versión**: 0.5.0  
**Última actualización**: 2025-12-14
