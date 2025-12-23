namespace Parhelion.Domain.Enums;

/// <summary>
/// Tipo de notificación para UI.
/// </summary>
public enum NotificationType
{
    /// <summary>Información general</summary>
    Info,
    
    /// <summary>Operación exitosa</summary>
    Success,
    
    /// <summary>Advertencia que requiere atención</summary>
    Warning,
    
    /// <summary>Alerta crítica que requiere acción inmediata</summary>
    Alert,
    
    /// <summary>Error del sistema</summary>
    Error
}

/// <summary>
/// Origen de la notificación (agente de IA o sistema).
/// </summary>
public enum NotificationSource
{
    /// <summary>Notificación generada por el sistema</summary>
    System,
    
    /// <summary>Agente de Crisis Management (excepciones de envío)</summary>
    CrisisManagement,
    
    /// <summary>Agente de Smart Booking (validación cargo-truck)</summary>
    SmartBooking,
    
    /// <summary>Agente de Fraud Prevention (QR Handshake)</summary>
    FraudPrevention,
    
    /// <summary>Notificación manual de admin</summary>
    Admin
}
