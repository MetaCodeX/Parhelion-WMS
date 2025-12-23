namespace Parhelion.Application.DTOs.Webhooks;

/// <summary>
/// Envelope base para todos los eventos de webhook.
/// Proporciona metadatos estándar para tracking y debugging.
/// </summary>
public record WebhookEvent(
    /// <summary>Tipo de evento (ej: "shipment.exception", "handshake.attempted")</summary>
    string EventType,
    /// <summary>Timestamp UTC del evento</summary>
    DateTime Timestamp,
    /// <summary>ID único para correlación de logs</summary>
    Guid CorrelationId,
    /// <summary>
    /// JWT firmado que n8n usa para autenticarse de vuelta al API.
    /// Contiene TenantId y expira en 15 minutos.
    /// Usar como: Authorization: Bearer {CallbackToken}
    /// </summary>
    string CallbackToken,
    /// <summary>Payload específico del evento</summary>
    object Payload
);

/// <summary>
/// Tipos de evento definidos para el sistema.
/// Usar estas constantes para consistencia.
/// </summary>
public static class WebhookEventTypes
{
    // ========== SHIPMENT EVENTS ==========
    public const string ShipmentCreated = "shipment.created";
    public const string ShipmentException = "shipment.exception";
    public const string ShipmentStatusChanged = "shipment.status_changed";
    public const string ShipmentAssigned = "shipment.assigned";
    
    // ========== BOOKING/VALIDATION EVENTS ==========
    public const string BookingRequested = "booking.requested";
    public const string BookingValidated = "booking.validated";
    public const string BookingRejected = "booking.rejected";
    
    // ========== QR HANDSHAKE EVENTS (Futuro) ==========
    public const string HandshakeAttempted = "handshake.attempted";
    public const string HandshakeValidated = "handshake.validated";
    public const string HandshakeFailed = "handshake.failed";
    
    // ========== CHECKPOINT EVENTS (Futuro) ==========
    public const string CheckpointCreated = "checkpoint.created";
    public const string CheckpointAnomalyDetected = "checkpoint.anomaly";
    
    // ========== DOCUMENT EVENTS (Futuro) ==========
    public const string DocumentGenerated = "document.generated";
    public const string DocumentSigned = "document.signed";
}
