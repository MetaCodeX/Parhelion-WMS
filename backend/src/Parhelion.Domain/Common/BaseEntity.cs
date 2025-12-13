namespace Parhelion.Domain.Common;

/// <summary>
/// Entidad base para todas las entidades del sistema.
/// Incluye Soft Delete y Audit Trail automático.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Soft Delete: true indica que la entidad fue eliminada lógicamente.
    /// </summary>
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// Entidad base para entidades que pertenecen a un tenant específico.
/// Hereda de BaseEntity para incluir Soft Delete y Audit Trail.
/// Todas las queries automáticamente filtran por TenantId via Query Filters.
/// </summary>
public abstract class TenantEntity : BaseEntity
{
    public Guid TenantId { get; set; }
}
