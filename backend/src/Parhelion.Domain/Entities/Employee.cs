using Parhelion.Domain.Common;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Datos de empleado para todos los roles (Admin, Driver, Warehouse).
/// Vinculado 1:1 con User cuando es empleado del tenant.
/// Centraliza datos legales que antes solo tenía Driver.
/// </summary>
public class Employee : TenantEntity
{
    public Guid UserId { get; set; }
    
    /// <summary>Teléfono personal del empleado</summary>
    public string Phone { get; set; } = null!;
    
    // ========== DATOS LEGALES (México) ==========
    
    /// <summary>RFC para nómina/facturación</summary>
    public string? Rfc { get; set; }
    
    /// <summary>Número de Seguro Social (IMSS)</summary>
    public string? Nss { get; set; }
    
    /// <summary>Clave Única de Registro de Población</summary>
    public string? Curp { get; set; }
    
    // ========== CONTACTO DE EMERGENCIA ==========
    
    /// <summary>Nombre del contacto de emergencia</summary>
    public string? EmergencyContact { get; set; }
    
    /// <summary>Teléfono del contacto de emergencia</summary>
    public string? EmergencyPhone { get; set; }
    
    // ========== INFORMACIÓN LABORAL ==========
    
    /// <summary>Fecha de contratación</summary>
    public DateTime? HireDate { get; set; }
    
    /// <summary>Turno asignado (nullable)</summary>
    public Guid? ShiftId { get; set; }
    
    /// <summary>Departamento: Admin, Operations, Field</summary>
    public string? Department { get; set; }
    
    // ========== NAVIGATION PROPERTIES ==========
    
    public Tenant Tenant { get; set; } = null!;
    public User User { get; set; } = null!;
    public Shift? Shift { get; set; }
    
    /// <summary>Extensión Driver (si es chofer)</summary>
    public Driver? Driver { get; set; }
    
    /// <summary>Extensión WarehouseOperator (si es almacenista)</summary>
    public WarehouseOperator? WarehouseOperator { get; set; }
}
