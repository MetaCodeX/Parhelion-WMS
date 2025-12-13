namespace Parhelion.Domain.Enums;

/// <summary>
/// Estatus del chofer.
/// </summary>
public enum DriverStatus
{
    /// <summary>Puede recibir nuevos envíos</summary>
    Available,
    
    /// <summary>Actualmente entregando paquetes</summary>
    OnRoute,
    
    /// <summary>No disponible (vacaciones, baja, etc.)</summary>
    Inactive
}
