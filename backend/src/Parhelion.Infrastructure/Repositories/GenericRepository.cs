using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.Interfaces;
using Parhelion.Domain.Common;
using Parhelion.Infrastructure.Data;

namespace Parhelion.Infrastructure.Repositories;

/// <summary>
/// Implementación genérica del Repository Pattern.
/// Maneja soft delete automáticamente.
/// </summary>
public class GenericRepository<TEntity> : IGenericRepository<TEntity> 
    where TEntity : BaseEntity
{
    protected readonly ParhelionDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public GenericRepository(ParhelionDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    // ========== READ OPERATIONS ==========

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;
        
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        
        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
        PagedRequest request,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (orderBy != null)
        {
            query = orderBy(query);
        }
        else
        {
            // Default: ordenar por CreatedAt descendente
            query = query.OrderByDescending(e => e.CreatedAt);
        }

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate == null 
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(predicate, cancellationToken);
    }

    // ========== WRITE OPERATIONS ==========

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }
        entity.CreatedAt = DateTime.UtcNow;
        
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }
            entity.CreatedAt = now;
        }
        
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.UpdatedAt = now;
        }
        _dbSet.UpdateRange(entities);
    }

    public virtual void Delete(TEntity entity)
    {
        // Soft delete
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }

    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = now;
        }
        _dbSet.UpdateRange(entities);
    }

    public virtual void HardDelete(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    // ========== QUERYABLE ACCESS ==========

    public IQueryable<TEntity> Query()
    {
        return _dbSet.AsQueryable();
    }

    public IQueryable<TEntity> QueryNoTracking()
    {
        return _dbSet.AsNoTracking();
    }
}

/// <summary>
/// Repository para entidades multi-tenant.
/// Agrega filtrado automático por TenantId.
/// </summary>
public class TenantRepository<TEntity> : GenericRepository<TEntity>, ITenantRepository<TEntity>
    where TEntity : TenantEntity
{
    public TenantRepository(ParhelionDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TEntity>> GetAllForTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> FindForTenantAsync(
        Guid tenantId,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.TenantId == tenantId)
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }
}
