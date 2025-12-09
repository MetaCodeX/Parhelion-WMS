# PARHELION-LOGISTICS | Documento de Requerimientos de Software

**Cliente:** Parhelion Logistics S.A. de C.V. (Monterrey, N.L.)
**Versión:** 2.3 (Final - Scope Freeze)
**Fecha:** Diciembre 2025
**Líder Técnico:** MetaCodeX

---

## 1. Visión del Proyecto

La empresa "Parhelion Logistics" actualmente gestiona su flota de camiones y envíos mediante hojas de cálculo de Excel y mensajes de WhatsApp. Esto ha provocado pérdida de paquetes, camiones viajando vacíos y falta de documentación legal para inspecciones.

**El Objetivo:** Desarrollar una Plataforma Unificada de Logística B2B (WMS + TMS) nivel Enterprise que permita administrar inventarios en almacén, flotillas tipificadas, redes de distribución Hub & Spoke, generar documentación legal mexicana (Carta Porte, POD) y rastrear el estatus de los envíos mediante checkpoints manuales.

**Objetivo Secundario (Demo Pública):** El sistema servirá también como **portafolio técnico interactivo**, permitiendo a reclutadores y visitantes probar el simulador sin afectar datos reales de producción.

---

## 2. Actores del Sistema (Usuarios)

El sistema debe soportar los siguientes tipos de usuarios con permisos distintos:

### A. Cliente (Dueño de la Operación)

- Es quien contrata el servicio de logística.
- Define su operación mediante un **formulario de onboarding** (ver Módulo 0).
- Puede tener múltiples camiones, choferes y ubicaciones bajo su cuenta.

### B. Administrador (Gerente de Tráfico)

- Tiene acceso total al sistema dentro de la cuenta del Cliente.
- Configura la red logística (Hubs, rutas, enlaces).
- Da de alta camiones tipificados y choferes.
- Crea envíos y asigna rutas predefinidas.
- Genera documentación B2B (Carta Porte, Manifiestos).

### C. Almacenista (Operador de Bodega)

- Acceso a la app de operaciones (React PWA en tablet).
- Gestiona la carga de camiones.
- Escanea códigos QR para transferencia de custodia.
- Valida peso y volumen antes de despacho.

### D. Chofer (Operador de Campo)

- Acceso a la app móvil (React PWA).
- Ve la Hoja de Ruta con sus paradas asignadas.
- Confirma llegadas a Hubs y entregas finales.
- Captura firma digital del receptor (POD).
- **Acción Crítica:** Es el único que puede cambiar el estatus de un envío a "Entregado".

### E. Visitante (Demo/Reclutador)

- Accede al sistema en **Modo Demo** sin registro.
- Puede probar todas las funcionalidades con datos simulados.
- Su progreso se guarda en una sesión temporal en BD que expira en 24-48 horas.

---

## 3. Requerimientos Funcionales (Módulos MVP)

### Módulo 0: Onboarding de Cliente

- [ ] **Registro de Cliente:** Formulario inicial para definir la empresa/operación.
- [ ] **Datos Requeridos:**
  - Nombre de la Empresa / Operación.
  - Cantidad de Camiones (flotilla).
  - Cantidad de Choferes disponibles.
- [ ] **Alta de Recursos:** Después del registro, el cliente puede dar de alta camiones, choferes y ubicaciones.
- [ ] **Multi-tenancy:** Cada cliente tiene su propio espacio aislado de datos.

### Módulo 1: Seguridad y Acceso

- [ ] **Login:** El sistema debe permitir el ingreso mediante Email y Contraseña.
- [ ] **Autenticación:** Debe usar tokens seguros (JWT). La sesión debe expirar automáticamente tras 2 horas de inactividad.
- [ ] **Roles:** Admin, Driver, Warehouse (Almacenista), DemoUser.
- [ ] **Protección:** Un Chofer no debe poder acceder a las pantallas de Administración (Rutas protegidas).
- [ ] **Recuperación de Contraseña:** Flujo básico de "Olvidé mi contraseña" con enlace temporal.

### Módulo 2: Gestión de Flotilla (Camiones)

- [ ] **Listado:** Ver todos los camiones disponibles, su placa, modelo, tipo y chofer asignado.
- [ ] **Alta de Camión:** Registrar placa (ej. "NL-554-X"), modelo, **Tipo de Camión** y capacidades.
- [ ] **Tipos de Camión (TruckType):**
  - `DryBox` - Caja Seca (Estándar)
  - `Refrigerated` - Termo/Refrigerado (Cadena de frío)
  - `HazmatTank` - Pipa (Materiales peligrosos)
  - `Flatbed` - Plataforma (Carga pesada)
  - `Armored` - Blindado (Alto valor)
- [ ] **Capacidades:** Peso máximo (kg) y volumen máximo (m³).
- [ ] **Validación:** No pueden existir dos camiones con la misma placa dentro del mismo Cliente.

### Módulo 2.5: Gestión de Choferes

- [ ] **Listado:** Ver todos los choferes registrados y su estatus (Disponible, En Ruta, Inactivo).
- [ ] **Alta de Chofer:** Registrar nombre, teléfono, email y licencia.
- [ ] **Asignación Híbrida Chofer-Camión:**
  - **default_truck_id:** Camión fijo asignado ("su unidad").
  - **current_truck_id:** Camión que conduce actualmente (puede diferir).
- [ ] **Bitácora de Flotilla (FleetLog):** Registro automático de cada cambio de vehículo con motivo (ShiftChange, Breakdown, Reassignment).

### Módulo 3: Red Logística (Locations)

- [ ] **Nodos de Red:** Gestión de ubicaciones con código único (estilo aeropuerto: MTY, GDL, MM).
- [ ] **Tipos de Ubicación (LocationType):**
  - `RegionalHub` - Nodo central, recibe y despacha masivo
  - `CrossDock` - Transferencia rápida sin almacenamiento
  - `Warehouse` - Bodega de almacenamiento prolongado
  - `Store` - Punto de venta final (solo recibe)
  - `SupplierPlant` - Fábrica de origen (solo despacha)
- [ ] **Capacidades:** Flags `can_receive` y `can_dispatch` por ubicación.

### Módulo 3.5: Enrutamiento (Hub & Spoke)

- [ ] **Enlaces de Red (NetworkLink):** Conexiones permitidas entre ubicaciones.
  - `FirstMile` - Recolección: Cliente/Proveedor → Hub
  - `LineHaul` - Carretera: Hub → Hub (larga distancia)
  - `LastMile` - Entrega: Hub → Cliente/Tienda
- [ ] **Regla de Conexión:** Clientes no pueden conectarse directamente entre sí.
- [ ] **Rutas Predefinidas (RouteBlueprint):** Secuencia de paradas con tiempos de tránsito.
- [ ] **Cálculo de ETA:** `scheduled_departure + SUM(transit_times)`.

### Módulo 4: Envíos (Shipments)

- [ ] **Crear Envío:** Registrar origen, destino, ruta asignada, destinatario y prioridad.
- [ ] **Flujo de Estados:**
  - `PendingApproval` - Orden de servicio esperando revisión
  - `Approved` - Envío aprobado, listo para asignar
  - `Loaded` - Paquete cargado en camión
  - `InTransit` - En movimiento entre ubicaciones
  - `AtHub` - Temporalmente en un centro de distribución
  - `OutForDelivery` - En camino al destinatario final
  - `Delivered` - Entrega confirmada, POD capturado
  - `Exception` - Problema que requiere atención

### Módulo 4.5: Manifiesto de Carga (ShipmentItems)

- [ ] **Partidas:** SKU, descripción, cantidad, dimensiones, peso.
- [ ] **Peso Volumétrico:** `(Largo × Ancho × Alto) / 5000`
- [ ] **Valor Declarado:** Para cálculo de seguro.
- [ ] **Flags Especiales:**
  - `is_fragile` - Requiere manejo cuidadoso
  - `is_hazardous` - Material peligroso (HAZMAT)
  - `requires_refrigeration` - Cadena de frío
- [ ] **Instrucciones de Estiba:** "No apilar más de 2 niveles".

### Módulo 5: Validación de Compatibilidad (Hard Constraints)

- [ ] **Cadena de Frío:** Items con `requires_refrigeration=true` SOLO en camiones `Refrigerated`.
- [ ] **HAZMAT:** Items con `is_hazardous=true` SOLO en camiones `HazmatTank`.
- [ ] **Alto Valor:** Si `SUM(declared_value) > $1,000,000`, requiere camión `Armored`.
- [ ] **Capacidad:** La suma de peso/volumen NO puede exceder la capacidad del camión.

### Módulo 6: Trazabilidad (Checkpoints)

- [ ] **Bitácora de Eventos:** Cada acción genera un `ShipmentCheckpoint`.
- [ ] **Códigos de Checkpoint:**
  - `Loaded` - Paquete cargado en camión (manual)
  - `QrScanned` - Paquete escaneado por chofer (cadena custodia)
  - `ArrivedHub` - Llegó a un Hub/CEDIS
  - `DepartedHub` - Salió del Hub hacia siguiente destino
  - `OutForDelivery` - En camino al destinatario final
  - `DeliveryAttempt` - Intento de entrega
  - `Delivered` - Entregado exitosamente
  - `Exception` - Problema reportado
- [ ] **Inmutabilidad:** Los checkpoints no se modifican, solo se agregan nuevos.

### Módulo 7: QR Handshake (Cadena de Custodia)

- [ ] **Generación de QR:** El sistema genera un QR único por envío.
- [ ] **Escaneo:** El Chofer usa la cámara de la app React para escanear.
- [ ] **Transferencia de Custodia:** El backend registra `was_qr_scanned=true` y crea checkpoint `QrScanned`.
- [ ] **Librerías:**
  - Angular: `angularx-qrcode`
  - React: `react-qr-reader`

### Módulo 8: Documentación B2B

- [ ] **Orden de Servicio:** Petición inicial del cliente a Admin.
- [ ] **Carta Porte (Waybill):** Documento legal SAT con QR para inspecciones.
- [ ] **Manifiesto de Carga:** Checklist de estiba para almacenista.
- [ ] **Hoja de Ruta (TripSheet):** Itinerario con ventanas de entrega.
- [ ] **POD (Proof of Delivery):** Firma digital del receptor, timestamp, incidencias.

### Módulo 9: Dashboard (Panel de Control)

- [ ] **KPIs Rápidos:** Mostrar tarjetas con:
  - Total de Envíos por Estado.
  - Choferes Disponibles vs En Ruta.
  - Ocupación de Flota.
  - Entregas del Día.
- [ ] **Timeline de Envío:** Visualización vertical tipo Metro.

### Módulo 10: Demo Pública / Modo Simulador

- [ ] **Acceso sin Registro:** Botón "Probar Demo" en la landing page.
- [ ] **Datos de Prueba:** Carga automática de datos ficticios (red de Hubs, camiones, choferes, envíos).
- [ ] **Sesión Temporal:** Cuenta temporal con UUID que expira en 24-48 horas.
- [ ] **Aislamiento:** Los datos de demo NO afectan a usuarios reales.
- [ ] **Uso Portfolio:** El sistema será visible para reclutadores como demostración de habilidades técnicas.

---

## 4. Requerimientos Técnicos (Non-Functional)

### Arquitectura y Backend

- **Lenguaje:** C# (.NET 8 Web API).
- **Patrón:** Clean Architecture (Domain, Infrastructure, Application, API).
- **Base de Datos:** PostgreSQL (Relacional).
- **ORM:** Entity Framework Core (Code First).

### Frontend (Híbrido)

| Área              | Tecnología      | Dispositivo | Usuario     |
| ----------------- | --------------- | ----------- | ----------- |
| **Control Tower** | Angular 18      | PC / Laptop | Admin       |
| **Operaciones**   | React (Web/PWA) | Tablet      | Almacenista |
| **Campo**         | React (Web/PWA) | Celular     | Chofer      |

### Infraestructura (DevOps)

- **Contenerización:** Docker y Docker Compose.
- **Servidor:** Digital Ocean Droplet (Linux Ubuntu/Arch).
- **Web Server:** Nginx como Proxy Inverso.
- **Dominio:** `api.macrostasis.lat` (Backend) y `macrostasis.lat` (Frontend).

---

## 5. Criterios de Aceptación (Definición de "Terminado")

El proyecto se considera exitoso si:

1. El código compila sin errores.
2. La base de datos se genera automáticamente con Migraciones.
3. Un usuario puede hacer el flujo completo: _Onboarding -> Login -> Configurar Red -> Crear Camión Tipificado -> Crear Chofer -> Crear Envío con Ruta -> Cargar (QR Scan) -> Tránsito por Hubs -> Entregar (POD)_.
4. El sistema valida compatibilidad de carga (refrigerado, HAZMAT, peso).
5. El sistema está desplegado en internet y es accesible públicamente.
6. Un reclutador puede acceder al **Modo Demo** y probar el sistema sin registrarse.

---

## 6. Roadmap: Funcionalidades Post-MVP (Fase 2)

> [!NOTE]
> Los siguientes módulos están **planificados para después del lanzamiento del MVP**. Se implementarán una vez que el sistema core esté estable y en producción.

### Módulo 11: Notificaciones y Mensajería (Serverless)

- [ ] **Infraestructura:** Integración con **Cloudflare Workers** + **Resend** API.
- [ ] **Alerta de Asignación:** Correo automático al Chofer con detalles del envío.
- [ ] **Reporte de Entrega:** Correo de confirmación al Cliente con fecha/hora exacta.

### Módulo 12: Asignación Inteligente (Smart Recommendations)

- [ ] **Algoritmo de Sugerencia:** Resaltar camiones recomendados basándose en:
  - Disponibilidad de carga (Peso/Volumen).
  - Tipo de camión compatible con la carga.
  - Coincidencia de Zona/Destino (Para agrupar envíos).

### Módulo 13: Reportes y Exportación Dinámica

- [ ] **Filtros de Fecha Flexibles:** Rango personalizado de fechas.
- [ ] **Exportación Excel (.xlsx):** ID, Cliente, Ruta, Chofer, Estatus, Timestamps.
- [ ] **Casos de Uso:** Reporte Diario, Proyección y Cierre de Mes.

---

## 7. Límites del Alcance (Scope & Limitations)

> [!IMPORTANT]
> Para cumplir con la fecha de entrega del MVP (Q1 2026), las siguientes funcionalidades están **explícitamente excluidas**:

| Funcionalidad              | Razón de Exclusión                                                     |
| -------------------------- | ---------------------------------------------------------------------- |
| **Pagos en Línea**         | Sistema operativo/logístico, no gestiona cobros ni facturación (SAT).  |
| **Rastreo GPS Real**       | Ubicación basada en confirmación manual de checkpoints, no telemetría. |
| **Chat en Vivo**           | Se usarán notificaciones por correo (Fase 2).                          |
| **Cálculo de Ruta Óptima** | Rutas predefinidas por Admin, no algoritmo de optimización automático. |

---

**Nota del Cliente:** "Necesitamos esto funcionando para el Q1 del 2026. La prioridad es la estabilidad de los datos, la validación de compatibilidad de carga y la facilidad de uso para los operadores."
