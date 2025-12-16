using System.Linq.Expressions;
using Parhelion.Application.DTOs.Common;
using Parhelion.Domain.Common;

namespace Parhelion.Application.Interfaces;

/// <summary>
/// Repository genérico para operaciones CRUD.
/// Implementa multi-tenancy y soft delete automáticamente.
/// </summary>
/// <typeparam name="TEntity">Tipo de entidad del dominio</typeparam>
public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    // ========== READ OPERATIONS ==========
    
    /// <summary>
    /// Obtiene una entidad por su ID.
    /// Respeta soft delete y multi-tenancy.
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene una entidad por su ID con includes.
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes);
    
    /// <summary>
    /// Obtiene todas las entidades (respeta query filters).
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene entidades paginadas.
    /// </summary>
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
        PagedRequest request,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca entidades que cumplan un predicado.
    /// </summary>
    Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene la primera entidad que cumpla un predicado.
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si existe al menos una entidad que cumpla el predicado.
    /// </summary>
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cuenta entidades que cumplan el predicado.
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    // ========== WRITE OPERATIONS ==========
    
    /// <summary>
    /// Agrega una nueva entidad.
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Agrega múltiples entidades.
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Actualiza una entidad existente.
    /// </summary>
    void Update(TEntity entity);
    
    /// <summary>
    /// Actualiza múltiples entidades.
    /// </summary>
    void UpdateRange(IEnumerable<TEntity> entities);
    
    /// <summary>
    /// Elimina una entidad (soft delete por defecto).
    /// </summary>
    void Delete(TEntity entity);
    
    /// <summary>
    /// Elimina múltiples entidades (soft delete).
    /// </summary>
    void DeleteRange(IEnumerable<TEntity> entities);
    
    /// <summary>
    /// Elimina físicamente una entidad (use con precaución).
    /// </summary>
    void HardDelete(TEntity entity);

    // ========== QUERYABLE ACCESS ==========
    
    /// <summary>
    /// Obtiene IQueryable para queries complejas.
    /// NOTA: Usar solo cuando los métodos anteriores no son suficientes.
    /// </summary>
    IQueryable<TEntity> Query();
    
    /// <summary>
    /// IQueryable sin tracking para lecturas.
    /// </summary>
    IQueryable<TEntity> QueryNoTracking();
}

/// <summary>
/// Repository específico para entidades multi-tenant.
/// Agrega filtrado automático por TenantId.
/// </summary>
/// <typeparam name="TEntity">Tipo de entidad del dominio</typeparam>
public interface ITenantRepository<TEntity> : IGenericRepository<TEntity> 
    where TEntity : TenantEntity
{
    /// <summary>
    /// Obtiene todas las entidades del tenant actual.
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllForTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Busca entidades del tenant actual.
    /// </summary>
    Task<IEnumerable<TEntity>> FindForTenantAsync(
        Guid tenantId,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
}
