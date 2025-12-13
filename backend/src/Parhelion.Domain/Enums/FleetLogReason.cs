namespace Parhelion.Domain.Enums;

/// <summary>
/// Razones de cambio de vehículo en FleetLog.
/// </summary>
public enum FleetLogReason
{
    /// <summary>Cambio de turno, entrega de unidad</summary>
    ShiftChange,
    
    /// <summary>Avería mecánica, cambio por emergencia</summary>
    Breakdown,
    
    /// <summary>Reasignación administrativa por disponibilidad</summary>
    Reassignment
}
