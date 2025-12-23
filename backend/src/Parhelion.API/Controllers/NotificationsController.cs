using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.API.Filters;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Notification;
using Parhelion.Application.Interfaces.Services;
using System.Security.Claims;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controller para gestión de notificaciones.
/// - POST: Autenticación con X-Service-Key (n8n/servicios)
/// - GET/PATCH: Autenticación con JWT (usuarios)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service)
    {
        _service = service;
    }

    // ========== PARA N8N Y SERVICIOS INTERNOS (API Key o CallbackToken) ==========

    /// <summary>
    /// Crea una nueva notificación.
    /// Autenticación: Authorization: Bearer {CallbackToken} o X-Service-Key
    /// 
    /// El TenantId se obtiene automáticamente del token JWT, NO del body.
    /// Esto simplifica la integración de n8n.
    /// </summary>
    [ServiceApiKey]
    [HttpPost]
    [ProducesResponseType(typeof(OperationResult<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateNotificationFromServiceRequest request,
        CancellationToken cancellationToken)
    {
        // Obtener TenantId del CallbackToken/ServiceApiKey (ya validado por el atributo)
        if (!HttpContext.Items.TryGetValue(ServiceApiKeyAttribute.TenantIdKey, out var tenantIdObj)
            || tenantIdObj is not Guid tenantId)
        {
            return Unauthorized(new { error = "TenantId not found in authentication token" });
        }

        // Crear el request completo con TenantId del token
        var fullRequest = new CreateNotificationRequest(
            TenantId: tenantId,
            UserId: request.UserId,
            RoleId: request.RoleId,
            Type: request.Type,
            Source: request.Source,
            Title: request.Title,
            Message: request.Message,
            MetadataJson: request.MetadataJson,
            RelatedEntityType: request.RelatedEntityType,
            RelatedEntityId: request.RelatedEntityId,
            Priority: request.Priority,
            RequiresAction: request.RequiresAction
        );

        var result = await _service.CreateAsync(fullRequest, cancellationToken);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    // ========== PARA APPS MÓVILES (JWT) ==========

    /// <summary>
    /// Obtiene notificaciones del usuario autenticado.
    /// </summary>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _service.GetByUserAsync(userId.Value, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el conteo de notificaciones no leídas.
    /// </summary>
    [Authorize]
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var count = await _service.GetUnreadCountAsync(userId.Value, cancellationToken);
        return Ok(new UnreadCountResponse(count));
    }

    /// <summary>
    /// Obtiene una notificación por ID.
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        
        if (result == null)
        {
            return NotFound();
        }

        // Validar que pertenece al usuario
        var userId = GetCurrentUserId();
        if (result.UserId != userId)
        {
            return Forbid();
        }

        return Ok(result);
    }

    /// <summary>
    /// Marca una notificación como leída.
    /// </summary>
    [Authorize]
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        // Validar que existe y pertenece al usuario
        var notification = await _service.GetByIdAsync(id, cancellationToken);
        if (notification == null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        if (notification.UserId != userId)
        {
            return Forbid();
        }

        var result = await _service.MarkAsReadAsync(id, cancellationToken);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok();
    }

    /// <summary>
    /// Marca todas las notificaciones del usuario como leídas.
    /// </summary>
    [Authorize]
    [HttpPost("mark-all-read")]
    [ProducesResponseType(typeof(OperationResult<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _service.MarkAllAsReadAsync(userId.Value, cancellationToken);
        return Ok(result);
    }

    // ========== HELPERS ==========

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? User.FindFirstValue("sub");
        
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        
        return null;
    }
}
