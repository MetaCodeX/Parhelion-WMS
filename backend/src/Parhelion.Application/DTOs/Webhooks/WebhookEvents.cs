namespace Parhelion.Application.DTOs.Webhooks;

/// <summary>
/// Evento: Un envío cambió a estado Exception.
/// Usado por: Agente de Crisis Management (n8n).
/// Propósito: Buscar rutas alternativas, reasignar vehículo, notificar gestor.
/// </summary>
public record ShipmentExceptionEvent(
    Guid ShipmentId,
    string TrackingNumber,
    Guid TenantId,
    
    // Ubicación actual
    Guid? CurrentLocationId,
    string? CurrentLocationCode,
    // Coordenadas del incidente (Crisis Management)
    decimal? Latitude,
    decimal? Longitude,
    
    // Destino
    Guid DestinationLocationId,
    string? DestinationLocationCode,
    
    // Tipo de carga (para priorización de IA)
    string CargoType,  // "Perishable", "Hazmat", "HighValue", "Standard"
    
    // ETAs
    DateTime? OriginalETA,
    DateTime? ScheduledDeparture,
    
    // Valores
    decimal? TotalDeclaredValue,
    decimal TotalWeightKg,
    decimal TotalVolumeM3,
    
    // Asignación actual
    Guid? DriverId,
    Guid? TruckId,
    string? TruckType,
    
    // Contexto adicional
    bool IsDelayed,
    string? ExceptionReason
);

/// <summary>
/// Evento: Nuevo envío creado, requiere validación de compatibilidad.
/// Usado por: Agente de Smart Booking (n8n).
/// Propósito: Validar que el tipo de carga sea compatible con el camión asignado.
/// </summary>
public record BookingRequestEvent(
    Guid ShipmentId,
    string TrackingNumber,
    Guid TenantId,
    
    // Capacidades requeridas
    decimal TotalWeightKg,
    decimal TotalVolumeM3,
    
    // Flags de carga especial
    bool HasRefrigeratedItems,
    bool HasHazmatItems,
    bool HasFragileItems,
    bool HasHighValueItems,
    
    // Valor total
    decimal TotalDeclaredValue,
    
    // Camión asignado (puede ser null si aún no se asigna)
    Guid? AssignedTruckId,
    string? AssignedTruckType,
    decimal? TruckMaxCapacityKg,
    decimal? TruckMaxVolumeM3
);

/// <summary>
/// Evento: Intento de QR Handshake para transferencia de custodia.
/// Usado por: Agente de Fraud Prevention (n8n).
/// Propósito: Validar geolocalización del chofer vs destino esperado.
/// 
/// NOTA: Este evento se activará cuando se implemente el módulo QR Handshake.
/// Por ahora está definido para que la infraestructura esté lista.
/// </summary>
public record HandshakeAttemptEvent(
    Guid ShipmentId,
    string TrackingNumber,
    Guid TenantId,
    
    // Chofer que intenta el handshake
    Guid DriverId,
    string? DriverName,
    
    // Ubicación reportada por el chofer
    decimal? DriverLatitude,
    decimal? DriverLongitude,
    DateTime ReportedTimestamp,
    
    // Ubicación esperada (destino)
    Guid DestinationLocationId,
    string? DestinationLocationCode,
    decimal? DestinationLatitude,
    decimal? DestinationLongitude,
    
    // Contexto del intento
    string? DriverReportedReason,  // "Normal", "Traffic", "Reroute", "Other"
    bool IsWithinGeofence,         // Calculado por el servicio antes de enviar
    double? DistanceFromDestinationKm
);

/// <summary>
/// Evento: Cambio de estatus de un envío (cualquier transición).
/// Usado por: Dashboard, Notificaciones, Analytics.
/// </summary>
public record ShipmentStatusChangedEvent(
    Guid ShipmentId,
    string TrackingNumber,
    Guid TenantId,
    string PreviousStatus,
    string NewStatus,
    DateTime ChangedAt,
    Guid? ChangedByUserId
);

/// <summary>
/// Evento: Checkpoint creado (llegada a hub, escaneo, etc.).
/// Usado por: Agentes de tracking, detección de anomalías.
/// 
/// NOTA: Se activará cuando se implemente el módulo de Checkpoints.
/// </summary>
public record CheckpointCreatedEvent(
    Guid CheckpointId,
    Guid ShipmentId,
    string TrackingNumber,
    Guid TenantId,
    
    // Datos del checkpoint
    string StatusCode,  // "Loaded", "ArrivedHub", "Delivered", etc.
    Guid? LocationId,
    string? LocationCode,
    DateTime Timestamp,
    
    // Quién lo creó
    Guid? HandledByDriverId,
    Guid? HandledByWarehouseOperatorId,
    
    // Contexto
    string? Remarks,
    bool WasQrScanned
);
