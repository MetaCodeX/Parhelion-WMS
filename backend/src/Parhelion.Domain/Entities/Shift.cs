using Parhelion.Domain.Common;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Turno de trabajo para empleados.
/// Permite asignar horarios predefinidos a los empleados del tenant.
/// </summary>
public class Shift : TenantEntity
{
    /// <summary>Nombre del turno (Matutino, Vespertino, Nocturno)</summary>
    public string Name { get; set; } = null!;
    
    /// <summary>Hora de inicio del turno</summary>
    public TimeOnly StartTime { get; set; }
    
    /// <summary>Hora de fin del turno</summary>
    public TimeOnly EndTime { get; set; }
    
    /// <summary>
    /// Días de la semana en que aplica el turno.
    /// Formato: "Mon,Tue,Wed,Thu,Fri" o "Sat,Sun"
    /// </summary>
    public string DaysOfWeek { get; set; } = "Mon,Tue,Wed,Thu,Fri";
    
    /// <summary>Si el turno está activo para asignación</summary>
    public bool IsActive { get; set; } = true;
    
    // ========== NAVIGATION PROPERTIES ==========
    
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
