using Parhelion.Domain.Common;
using Parhelion.Domain.Enums;

namespace Parhelion.Domain.Entities;

/// <summary>
/// Notificación generada por los agentes de IA de n8n.
/// Almacena alertas, excepciones y eventos importantes para usuarios.
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>
    /// Tenant propietario de la notificación.
    /// </summary>
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;
    
    /// <summary>
    /// Usuario destinatario (null = broadcast a rol).
    /// </summary>
    public Guid? UserId { get; set; }
    public virtual User? User { get; set; }
    
    /// <summary>
    /// Rol destinatario si UserId es null.
    /// </summary>
    public Guid? RoleId { get; set; }
    public virtual Role? Role { get; set; }
    
    /// <summary>
    /// Tipo de notificación (Alert, Warning, Info, Success).
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.Info;
    
    /// <summary>
    /// Agente de IA que generó la notificación.
    /// </summary>
    public NotificationSource Source { get; set; } = NotificationSource.System;
    
    /// <summary>
    /// Título corto de la notificación.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Mensaje detallado.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Datos adicionales en JSON (shipmentId, routeId, etc.).
    /// </summary>
    public string? MetadataJson { get; set; }
    
    /// <summary>
    /// Tipo de entidad relacionada para deep linking.
    /// </summary>
    public string? RelatedEntityType { get; set; }
    
    /// <summary>
    /// ID de entidad relacionada (ej: ShipmentId).
    /// </summary>
    public Guid? RelatedEntityId { get; set; }
    
    /// <summary>
    /// Si la notificación ha sido leída.
    /// </summary>
    public bool IsRead { get; set; } = false;
    
    /// <summary>
    /// Timestamp de lectura.
    /// </summary>
    public DateTime? ReadAt { get; set; }
    
    /// <summary>
    /// Prioridad (1=crítica, 5=baja).
    /// </summary>
    public int Priority { get; set; } = 3;
    
    /// <summary>
    /// Si requiere acción del usuario.
    /// </summary>
    public bool RequiresAction { get; set; } = false;
    
    /// <summary>
    /// Acción completada por el usuario.
    /// </summary>
    public bool ActionCompleted { get; set; } = false;
}
