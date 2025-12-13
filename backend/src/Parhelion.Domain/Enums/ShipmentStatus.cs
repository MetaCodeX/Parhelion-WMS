namespace Parhelion.Domain.Enums;

/// <summary>
/// Estados del envío durante su ciclo de vida.
/// </summary>
public enum ShipmentStatus
{
    /// <summary>Orden de servicio esperando revisión</summary>
    PendingApproval,
    
    /// <summary>Envío aprobado, listo para asignar</summary>
    Approved,
    
    /// <summary>Paquete cargado en camión, listo para salir</summary>
    Loaded,
    
    /// <summary>En movimiento entre ubicaciones</summary>
    InTransit,
    
    /// <summary>Temporalmente en un centro de distribución</summary>
    AtHub,
    
    /// <summary>En camino al destinatario final</summary>
    OutForDelivery,
    
    /// <summary>Entrega confirmada, POD capturado</summary>
    Delivered,
    
    /// <summary>Problema que requiere atención</summary>
    Exception
}
