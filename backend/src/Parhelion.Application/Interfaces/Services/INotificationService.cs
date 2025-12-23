using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Notification;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de notificaciones.
/// Soporta creación desde n8n y lectura desde apps móviles.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Crea una notificación (usado por n8n y backend interno).
    /// </summary>
    Task<OperationResult<NotificationResponse>> CreateAsync(
        CreateNotificationRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene notificaciones paginadas de un usuario.
    /// </summary>
    Task<PagedResult<NotificationResponse>> GetByUserAsync(
        Guid userId, 
        PagedRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene notificaciones paginadas por rol (broadcast).
    /// </summary>
    Task<PagedResult<NotificationResponse>> GetByRoleAsync(
        Guid tenantId,
        Guid roleId, 
        PagedRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una notificación por ID.
    /// </summary>
    Task<NotificationResponse?> GetByIdAsync(
        Guid notificationId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cuenta notificaciones no leídas del usuario.
    /// </summary>
    Task<int> GetUnreadCountAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca una notificación como leída.
    /// </summary>
    Task<OperationResult> MarkAsReadAsync(
        Guid notificationId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca todas las notificaciones del usuario como leídas.
    /// </summary>
    Task<OperationResult<int>> MarkAllAsReadAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
}
