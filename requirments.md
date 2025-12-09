# PARHELION-WMS | Documento de Requerimientos de Software

**Cliente:** Parhelion Logistics S.A. de C.V. (Monterrey, N.L.)
**Versión:** 1.3 (MVP - Minimum Viable Product)
**Fecha:** Diciembre 2025
**Líder Técnico:** MetaCodeX

---

## 1. Visión del Proyecto

La empresa "Parhelion Logistics" actualmente gestiona su flota de camiones y envíos mediante hojas de cálculo de Excel y mensajes de WhatsApp. Esto ha provocado pérdida de paquetes y camiones viajando vacíos.

**El Objetivo:** Desarrollar una plataforma Web centralizada (WMS) que permita administrar la flotilla, asignar cargas y rastrear el estatus de los envíos en tiempo real. El sistema debe ser robusto, seguro y accesible desde dispositivos móviles para los choferes.

**Objetivo Secundario (Demo Pública):** El sistema servirá también como **portafolio técnico interactivo**, permitiendo a reclutadores y visitantes probar el simulador sin afectar datos reales de producción.

---

## 2. Actores del Sistema (Usuarios)

El sistema debe soportar los siguientes tipos de usuarios con permisos distintos:

### A. Cliente (Dueño de la Operación)

- Es quien contrata el servicio de logística.
- Define su operación mediante un **formulario de onboarding** (ver Módulo 0).
- Puede tener múltiples camiones y choferes bajo su cuenta.

### B. Administrador (Gerente de Tráfico)

- Tiene acceso total al sistema dentro de la cuenta del Cliente.
- Puede dar de alta camiones y choferes.
- Crea los envíos.
- Asigna los envíos a los camiones y choferes.

### C. Chofer (Operador)

- Tiene acceso limitado (solo lectura de su carga).
- Puede ver los envíos que tiene asignados para hoy.
- **Acción Crítica:** Es el único que puede cambiar el estatus de un envío a "Entregado".

### D. Visitante (Demo/Reclutador)

- Accede al sistema en **Modo Demo** sin registro.
- Puede probar todas las funcionalidades con datos simulados.
- Su progreso se guarda localmente (caché/localStorage) o en una sesión temporal en BD.

---

## 3. Requerimientos Funcionales (Módulos MVP)

### Módulo 0: Onboarding de Cliente

- [ ] **Registro de Cliente:** Formulario inicial para definir la empresa/operación.
- [ ] **Datos Requeridos:**
  - Nombre de la Empresa / Operación.
  - Cantidad de Camiones (flotilla).
  - Cantidad de Choferes disponibles.
- [ ] **Alta de Recursos:** Después del registro, el cliente puede dar de alta cada camión y cada chofer con su información detallada.
- [ ] **Multi-tenancy:** Cada cliente tiene su propio espacio aislado de datos.

### Módulo 1: Seguridad y Acceso

- [ ] **Login:** El sistema debe permitir el ingreso mediante Email y Contraseña.
- [ ] **Autenticación:** Debe usar tokens seguros (JWT). La sesión debe expirar automáticamente tras 2 horas de inactividad.
- [ ] **Protección:** Un Chofer no debe poder acceder a las pantallas de Administración (Rutas protegidas).
- [ ] **Recuperación de Contraseña:** Flujo básico de "Olvidé mi contraseña" con enlace temporal.

### Módulo 2: Gestión de Flotilla (Camiones)

- [ ] **Listado:** Ver todos los camiones disponibles, su placa, modelo y chofer asignado.
- [ ] **Alta de Camión:** Registrar placa (ej. "NL-554-X"), modelo y **Capacidad Máxima de Carga (kg)**.
- [ ] **Validación:** No pueden existir dos camiones con la misma placa dentro del mismo Cliente.

### Módulo 2.5: Gestión de Choferes

- [ ] **Listado:** Ver todos los choferes registrados y su estatus (Disponible, En Ruta, Inactivo).
- [ ] **Alta de Chofer:** Registrar nombre, teléfono, email y licencia.
- [ ] **Asignación Híbrida Chofer-Camión:**
  - **Modo Fijo:** Un chofer puede tener un camión asignado permanentemente ("su unidad").
  - **Modo Dinámico:** El Admin puede reasignar un chofer a otro camión según disponibilidad o urgencia del envío.
  - El sistema debe mostrar claramente qué choferes están libres y cuáles están en ruta.

### Módulo 3: Logística (Envíos)

- [ ] **Crear Envío:** Registrar Destinatario, Dirección Origen, Dirección Destino, **Peso del Paquete (kg)** y **Prioridad (Normal/Urgente)**.
- [ ] **Estatus del Envío:** Todo envío nace con el estatus `En Almacén`.
- [ ] **Asignación (Lógica de Negocio):**
  - El Administrador asigna un envío a un camión **y** a un chofer.
  - El estatus cambia automáticamente a `En Ruta`.
  - **REGLA DE ORO (Validación de Peso):** El sistema **NO** debe permitir asignar un paquete a un camión si la suma de pesos supera la capacidad máxima del vehículo.
  - **REGLA DE URGENCIA:** Los envíos marcados como `Urgente` deben priorizarse visualmente y el sistema puede sugerir choferes disponibles inmediatamente.

### Módulo 4: Operación en Campo (App Chofer)

- [ ] **Mis Envíos:** El chofer ve una lista filtrada solo con _sus_ paquetes asignados.
- [ ] **Confirmar Entrega:** Botón para marcar el envío como `Entregado`. Esto registra la fecha y hora exacta del evento.

### Módulo 5: Dashboard (Panel de Control)

- [ ] **KPIs Rápidos:** Mostrar tarjetas con:
  - Total de Envíos en Almacén (Pendientes).
  - Total de Envíos en Ruta (Activos).
  - Total de Envíos Entregados (Histórico).
  - Choferes Disponibles vs En Ruta.

### Módulo 10: Demo Pública / Modo Simulador

- [ ] **Acceso sin Registro:** Botón "Probar Demo" en la landing page.
- [ ] **Datos de Prueba:** Carga automática de datos ficticios (camiones, choferes, envíos de ejemplo).
- [ ] **Persistencia de Sesión:**
  - **Opción A (LocalStorage):** El progreso del usuario se guarda en el navegador (sin backend).
  - **Opción B (Sesión Temporal en BD):** Se crea una cuenta temporal con UUID que expira en 24-48 horas.
- [ ] **Aislamiento:** Los datos de demo NO afectan a usuarios reales.
- [ ] **Uso Portfolio:** El sistema será visible para reclutadores como demostración de habilidades técnicas.

---

## 4. Requerimientos Técnicos (Non-Functional)

### Arquitectura y Backend

- **Lenguaje:** C# (.NET 8 Web API).
- **Patrón:** Clean Architecture (Domain, Infrastructure, Application, API).
- **Base de Datos:** PostgreSQL (Relacional).
- **ORM:** Entity Framework Core (Code First).

### Frontend (Web)

- **Framework:** Angular 18+.
- **UI Kit:** Angular Material o PrimeNG.
- **Diseño:** Responsivo (Debe verse bien en la Laptop del Admin y en el Celular del Chofer).

### Infraestructura (DevOps)

- **Contenerización:** Docker y Docker Compose.
- **Servidor:** Digital Ocean Droplet (Linux Ubuntu/Arch).
- **Web Server:** Nginx como Proxy Inverso.
- **Dominio:** `api.macrostasis.lat` (Backend) y `macrostasis.lat` (Frontend).

---

## 5. Criterios de Aceptación (Definición de "Terminado")

El proyecto se considera exitoso si:

1.  El código compila sin errores.
2.  La base de datos se genera automáticamente con Migraciones.
3.  Un usuario puede hacer el flujo completo: _Onboarding -> Login -> Crear Camión -> Crear Chofer -> Crear Paquete -> Asignar -> Entregar_.
4.  El sistema está desplegado en internet y es accesible públicamente.
5.  Un reclutador puede acceder al **Modo Demo** y probar el sistema sin registrarse.

---

## 6. Roadmap: Funcionalidades Post-MVP (Fase 2)

> [!NOTE]
> Los siguientes módulos están **planificados para después del lanzamiento del MVP**. Se implementarán una vez que el sistema core esté estable y en producción.

### Módulo 6: Notificaciones y Mensajería (Serverless)

El sistema utilizará una arquitectura desacoplada para el envío de correos transaccionales.

- [ ] **Infraestructura:** Integración con **Cloudflare Workers** + **Resend** API.
- [ ] **Alerta de Asignación:** Correo automático al Chofer con detalles del envío.
- [ ] **Reporte de Entrega:** Correo de confirmación al Cliente con fecha/hora exacta.

### Módulo 7: Asignación Inteligente (Smart Recommendations)

- [ ] **Algoritmo de Sugerencia:** Resaltar camiones recomendados basándose en:
  - Disponibilidad de carga (Peso).
  - Coincidencia de Zona/Destino (Para agrupar envíos).

### Módulo 8: Reportes y Exportación Dinámica

- [ ] **Filtros de Fecha Flexibles:** Rango personalizado de fechas.
- [ ] **Exportación Excel (.xlsx):** ID, Cliente, Direcciones, Chofer, Estatus, Fechas.
- [ ] **Casos de Uso:** Reporte Diario, Proyección y Cierre de Mes.

### Módulo 9: Comprobante Digital de Entrega (POD)

- [ ] **Generación Automática:** PDF con diseño corporativo al entregar.
- [ ] **Contenido:** Folio UUID, Peso, Trazabilidad (Cliente -> Chofer -> Destinatario), Timestamp.
- [ ] **Accesibilidad:** Descargable por Chofer y Admin.

---

## 7. Límites del Alcance (Scope & Limitations)

> [!IMPORTANT]
> Para cumplir con la fecha de entrega del MVP (Q1 2026), las siguientes funcionalidades están **explícitamente excluidas**:

| Funcionalidad        | Razón de Exclusión                                                    |
| -------------------- | --------------------------------------------------------------------- |
| **Pagos en Línea**   | Sistema operativo/logístico, no gestiona cobros ni facturación (SAT). |
| **Rastreo GPS Real** | Ubicación basada en estatus manual, no telemetría GPS física.         |
| **Chat en Vivo**     | Se usarán notificaciones por correo (Fase 2).                         |

---

**Nota del Cliente:** "Necesitamos esto funcionando para el Q1 del 2026. La prioridad es la estabilidad de los datos y la facilidad de uso para los choferes."
