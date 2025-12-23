namespace Parhelion.Application.DTOs.Notification;

/// <summary>
/// Response DTO para notificaciones.
/// </summary>
public record NotificationResponse(
    Guid Id,
    Guid TenantId,
    Guid? UserId,
    Guid? RoleId,
    string Type,
    string Source,
    string Title,
    string Message,
    string? MetadataJson,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    bool IsRead,
    DateTime? ReadAt,
    int Priority,
    bool RequiresAction,
    bool ActionCompleted,
    DateTime CreatedAt
);

/// <summary>
/// Request para crear notificación (usado por n8n y backend).
/// </summary>
public record CreateNotificationRequest(
    Guid TenantId,
    Guid? UserId,
    Guid? RoleId,
    string Type,
    string Source,
    string Title,
    string Message,
    string? MetadataJson = null,
    string? RelatedEntityType = null,
    Guid? RelatedEntityId = null,
    int Priority = 3,
    bool RequiresAction = false
);

/// <summary>
/// Request simplificado para n8n y servicios externos.
/// El TenantId se obtiene del CallbackToken JWT, no del body.
/// </summary>
public record CreateNotificationFromServiceRequest(
    Guid? UserId,
    Guid? RoleId,
    string Type,
    string Source,
    string Title,
    string Message,
    string? MetadataJson = null,
    string? RelatedEntityType = null,
    Guid? RelatedEntityId = null,
    int Priority = 3,
    bool RequiresAction = false
);

/// <summary>
/// Respuesta para contador de no leídas.
/// </summary>
public record UnreadCountResponse(int Count);

/// <summary>
/// Request para marcar notificaciones como leídas.
/// </summary>
public record MarkAsReadRequest(Guid NotificationId);
