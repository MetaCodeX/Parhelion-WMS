using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Notification;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Services.Notification;

/// <summary>
/// Implementación del servicio de notificaciones.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<OperationResult<NotificationResponse>> CreateAsync(
        CreateNotificationRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validar que el tenant existe
            var tenantExists = await _unitOfWork.Tenants
                .AnyAsync(t => t.Id == request.TenantId, cancellationToken);
            
            if (!tenantExists)
            {
                return OperationResult<NotificationResponse>.Fail("Tenant not found");
            }

            // Validar usuario si se proporciona
            if (request.UserId.HasValue)
            {
                var userExists = await _unitOfWork.Users
                    .AnyAsync(u => u.Id == request.UserId.Value, cancellationToken);
                
                if (!userExists)
                {
                    return OperationResult<NotificationResponse>.Fail("User not found");
                }
            }

            // Parsear enums
            if (!Enum.TryParse<NotificationType>(request.Type, true, out var type))
            {
                return OperationResult<NotificationResponse>.Fail($"Invalid notification type: {request.Type}");
            }

            if (!Enum.TryParse<NotificationSource>(request.Source, true, out var source))
            {
                return OperationResult<NotificationResponse>.Fail($"Invalid notification source: {request.Source}");
            }

            // Crear entidad
            var entity = new Domain.Entities.Notification
            {
                TenantId = request.TenantId,
                UserId = request.UserId,
                RoleId = request.RoleId,
                Type = type,
                Source = source,
                Title = request.Title,
                Message = request.Message,
                MetadataJson = request.MetadataJson,
                RelatedEntityType = request.RelatedEntityType,
                RelatedEntityId = request.RelatedEntityId,
                Priority = request.Priority,
                RequiresAction = request.RequiresAction,
                IsRead = false
            };

            await _unitOfWork.Notifications.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Notification created: {Id} for User:{UserId} Role:{RoleId} Source:{Source}", 
                entity.Id, request.UserId, request.RoleId, request.Source);

            return OperationResult<NotificationResponse>.Ok(MapToResponse(entity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return OperationResult<NotificationResponse>.Fail($"Error creating notification: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<PagedResult<NotificationResponse>> GetByUserAsync(
        Guid userId, 
        PagedRequest request, 
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Notifications.Query()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => MapToResponse(n))
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationResponse>(
            items, totalCount, request.Page, request.PageSize);
    }

    /// <inheritdoc />
    public async Task<PagedResult<NotificationResponse>> GetByRoleAsync(
        Guid tenantId,
        Guid roleId, 
        PagedRequest request, 
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Notifications.Query()
            .Where(n => n.TenantId == tenantId && n.RoleId == roleId)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => MapToResponse(n))
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationResponse>(
            items, totalCount, request.Page, request.PageSize);
    }

    /// <inheritdoc />
    public async Task<NotificationResponse?> GetByIdAsync(
        Guid notificationId, 
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);

        return entity != null ? MapToResponse(entity) : null;
    }

    /// <inheritdoc />
    public async Task<int> GetUnreadCountAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Notifications.Query()
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OperationResult> MarkAsReadAsync(
        Guid notificationId, 
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);

        if (entity == null)
        {
            return OperationResult.Fail("Notification not found");
        }

        entity.IsRead = true;
        entity.ReadAt = DateTime.UtcNow;
        
        _unitOfWork.Notifications.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult.Ok();
    }

    /// <inheritdoc />
    public async Task<OperationResult<int>> MarkAllAsReadAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _unitOfWork.Notifications.Query()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
            _unitOfWork.Notifications.Update(notification);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Ok(unreadNotifications.Count);
    }

    private static NotificationResponse MapToResponse(Domain.Entities.Notification entity)
    {
        return new NotificationResponse(
            entity.Id,
            entity.TenantId,
            entity.UserId,
            entity.RoleId,
            entity.Type.ToString(),
            entity.Source.ToString(),
            entity.Title,
            entity.Message,
            entity.MetadataJson,
            entity.RelatedEntityType,
            entity.RelatedEntityId,
            entity.IsRead,
            entity.ReadAt,
            entity.Priority,
            entity.RequiresAction,
            entity.ActionCompleted,
            entity.CreatedAt
        );
    }
}
