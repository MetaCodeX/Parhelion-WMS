namespace Parhelion.Domain.Enums;

/// <summary>
/// Estados de checkpoint para trazabilidad de envíos.
/// </summary>
public enum CheckpointStatus
{
    /// <summary>Paquete cargado en camión (manual)</summary>
    Loaded,
    
    /// <summary>Paquete escaneado por chofer (cadena custodia)</summary>
    QrScanned,
    
    /// <summary>Llegó a un Hub/CEDIS</summary>
    ArrivedHub,
    
    /// <summary>Salió del Hub hacia siguiente destino</summary>
    DepartedHub,
    
    /// <summary>En camino al destinatario final</summary>
    OutForDelivery,
    
    /// <summary>Intento de entrega (puede incluir motivo)</summary>
    DeliveryAttempt,
    
    /// <summary>Entregado exitosamente</summary>
    Delivered,
    
    /// <summary>Problema: dirección incorrecta, rechazo, etc.</summary>
    Exception
}
