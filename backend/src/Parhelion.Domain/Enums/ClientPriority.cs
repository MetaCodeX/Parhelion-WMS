namespace Parhelion.Domain.Enums;

/// <summary>
/// Prioridad predeterminada para entregas del cliente.
/// Define la urgencia con la que normalmente se deben entregar los envíos de este cliente.
/// Nota: Cada envío puede tener su propia prioridad en ShipmentPriority.
/// </summary>
public enum ClientPriority
{
    /// <summary>Prioridad normal - Entrega estándar (3-5 días)</summary>
    Normal = 1,
    
    /// <summary>Prioridad baja - Sin urgencia, puede esperar</summary>
    Low = 2,
    
    /// <summary>Prioridad alta - Entregas más rápidas (1-2 días)</summary>
    High = 3,
    
    /// <summary>Urgente - Entregas express/mismo día</summary>
    Urgent = 4
}
