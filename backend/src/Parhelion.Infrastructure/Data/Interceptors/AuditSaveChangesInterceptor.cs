using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Parhelion.Application.Services;
using Parhelion.Domain.Common;

namespace Parhelion.Infrastructure.Data.Interceptors;

/// <summary>
/// Interceptor que automáticamente llena campos de auditoría antes de guardar.
/// - CreatedAt, CreatedByUserId en inserts
/// - UpdatedAt, LastModifiedByUserId en updates
/// - DeletedAt en soft deletes
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    
    public AuditSaveChangesInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }
    
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, 
        InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }
    
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    
    private void UpdateAuditFields(DbContext? context)
    {
        if (context == null) return;
        
        var now = DateTime.UtcNow;
        var userId = _currentUserService.UserId;
        var tenantId = _currentUserService.TenantId;
        
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedByUserId = userId;
                    entry.Entity.IsDeleted = false;
                    
                    // Assign TenantId automatically for tenant entities
                    if (entry.Entity is TenantEntity tenantEntity && tenantId.HasValue)
                    {
                        if (tenantEntity.TenantId == Guid.Empty)
                        {
                            tenantEntity.TenantId = tenantId.Value;
                        }
                    }
                    break;
                    
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.LastModifiedByUserId = userId;
                    
                    // Soft delete timestamp
                    if (entry.Entity.IsDeleted && entry.Entity.DeletedAt == null)
                    {
                        entry.Entity.DeletedAt = now;
                    }
                    break;
            }
        }
    }
}
