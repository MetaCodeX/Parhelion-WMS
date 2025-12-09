# Parhelion-Logistics

![Parhelion-Logistics Banner](./bannerlogo.png)

![.NET 8](https://img.shields.io/badge/.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![React](https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

Plataforma Unificada de Logística B2B (WMS + TMS) nivel Enterprise. Gestiona inventarios, flotas tipificadas, redes Hub & Spoke y documentación legal (Carta Porte) en un entorno Multi-tenant.

> **Estado del Proyecto:** Diseño Finalizado (v2.3) - Listo para Implementación

---

## Descripción

**Parhelion-Logistics** es una plataforma SaaS multi-tenant de nivel Enterprise que unifica las capacidades de un WMS (Warehouse Management System) y un TMS (Transportation Management System). Diseñada para empresas de transporte B2B que requieren gestión integral: inventarios estáticos en almacén, flotas tipificadas (refrigerado, HAZMAT, blindado), redes de distribución Hub & Spoke, trazabilidad por checkpoints y documentación legal mexicana (Carta Porte, POD).

**Objetivo Técnico:** Implementación de **Clean Architecture** y **Domain-Driven Design (DDD)** en un entorno de producción utilizando .NET 8, Angular, React, Docker y PostgreSQL.

---

## Características (MVP Final)

### Core

- [x] Documentación de requerimientos y esquema de base de datos
- [ ] **Arquitectura Base:** Configuración de Clean Architecture y estructura de proyecto
- [ ] **Multi-tenancy:** Aislamiento de datos por cliente/empresa

### Gestión de Flotilla

- [ ] **Camiones Tipificados:** DryBox, Refrigerado, HAZMAT, Plataforma, Blindado
- [ ] **Choferes:** Asignación fija (default_truck) y dinámica (current_truck)
- [ ] **Bitácora de Flotilla:** Historial de cambios de vehículo (FleetLog)

### Red Logística (Hub & Spoke)

- [ ] **Nodos de Red:** RegionalHub, CrossDock, Warehouse, Store, SupplierPlant
- [ ] **Códigos Aeroportuarios:** Identificadores únicos por ubicación (MTY, GDL, MM)
- [ ] **Enlaces de Red:** Conexiones FirstMile, LineHaul, LastMile
- [ ] **Rutas Predefinidas:** RouteBlueprint con paradas y tiempos de tránsito

### Envíos y Trazabilidad

- [ ] **Manifiesto de Carga:** Items con peso volumétrico y valor declarado
- [ ] **Restricciones de Compatibilidad:** Cadena de frío, HAZMAT, Alto valor
- [ ] **Checkpoints:** Bitácora de eventos (Loaded, QrScanned, ArrivedHub, Delivered)
- [ ] **QR Handshake:** Transferencia de custodia digital mediante escaneo

### Documentación B2B

- [ ] **Orden de Servicio:** Petición inicial del cliente
- [ ] **Carta Porte (Waybill):** Documento legal SAT para transporte
- [ ] **Manifiesto de Carga:** Checklist de estiba para almacenista
- [ ] **Hoja de Ruta:** Itinerario con ventanas de entrega
- [ ] **POD (Proof of Delivery):** Firma digital del receptor

### Operación

- [ ] **Seguridad:** Autenticación JWT con roles (Admin/Chofer/Almacenista)
- [ ] **Dashboard:** KPIs operativos en tiempo real
- [ ] **Modo Demo:** Acceso para reclutadores sin registro previo

---

## Stack Tecnológico

| Capa                     | Tecnología                            | Usuario        |
| :----------------------- | :------------------------------------ | :------------- |
| **Backend**              | C# / .NET 8 Web API                   | -              |
| **Base de Datos**        | PostgreSQL 16                         | -              |
| **ORM**                  | Entity Framework Core (Code First)    | -              |
| **Frontend (Admin)**     | Angular 18+ (Material Design)         | Admin          |
| **Frontend (Operación)** | React (PWA)                           | Chofer/Almacén |
| **Infraestructura**      | Docker Compose, Nginx (Reverse Proxy) | -              |
| **Hosting**              | Digital Ocean Droplet (Linux)         | -              |

---

## Arquitectura

El proyecto sigue estrictamente **Clean Architecture** para desacoplar la lógica de negocio de la infraestructura:

```mermaid
graph TD
    User[Cliente / API Request] --> API
    subgraph "Core"
        Domain[Domain Layer<br>Entidades y Reglas de Negocio]
        App[Application Layer<br>Interfaces y Casos de Uso]
    end
    subgraph "External"
        Infra[Infrastructure Layer<br>EF Core, Servicios Externos]
    end

    API --> App
    App --> Domain
    API --> Infra
    Infra --> App
    Infra --> Domain
```

### Topología de Red (Hub & Spoke)

```mermaid
graph TD
    subgraph "Proveedores"
        A[EMP-A]
        B[EMP-B]
    end
    subgraph "Red de Hubs"
        MM((MM))
        CC((CC))
    end
    subgraph "Clientes"
        G[EMP-G]
    end
    B -->|FirstMile| MM
    MM ==>|LineHaul| CC
    CC -->|LastMile| G
```

---

## Estructura del Proyecto

```
src/
├── Parhelion.Domain/         # Núcleo: Entidades y Excepciones (Sin dependencias)
├── Parhelion.Application/    # Reglas: DTOs, Interfaces, Validaciones
├── Parhelion.Infrastructure/ # Persistencia: DbContext, Repositorios, Migraciones
└── Parhelion.API/            # Entrada: Controllers, JWT Config, DI
```

---

## Documentación

| Documento                                        | Descripción                                   |
| :----------------------------------------------- | :-------------------------------------------- |
| [Requerimientos (MVP)](./requirments.md)         | Especificación funcional completa del sistema |
| [Esquema de Base de Datos](./database-schema.md) | Diagrama ER, entidades y reglas de negocio    |

---

## Entidades Principales

| Entidad              | Descripción                                    |
| :------------------- | :--------------------------------------------- |
| `Tenant`             | Cliente/Empresa (Multi-tenancy)                |
| `Location`           | Nodo de red con código único (Hub, Store, etc) |
| `Truck`              | Vehículo tipificado con capacidad              |
| `Driver`             | Chofer con camión fijo y actual                |
| `Shipment`           | Envío con ruta asignada y documentos           |
| `ShipmentItem`       | Partida del manifiesto con peso volumétrico    |
| `ShipmentCheckpoint` | Evento de trazabilidad                         |
| `RouteBlueprint`     | Ruta predefinida con paradas                   |
| `NetworkLink`        | Conexión entre nodos (adyacencia)              |

---

## Despliegue

| Servicio        | URL                   |
| :-------------- | :-------------------- |
| **API Backend** | `api.macrostasis.lat` |
| **Frontend**    | `macrostasis.lat`     |

---

## Autor

**MetaCodeX** | 2025

_Proyecto desarrollado como portafolio profesional de Arquitectura de Software y Sistemas Logísticos B2B._
