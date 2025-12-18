using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Enums;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de envíos.
/// Endpoints para CRUD, tracking, asignación y workflow de estados.
/// </summary>
[ApiController]
[Route("api/shipments")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipmentService;

    /// <summary>
    /// Inicializa el controlador con el servicio de Shipments.
    /// </summary>
    public ShipmentsController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    /// <summary>
    /// Obtiene todos los envíos con paginación.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ShipmentResponse>>> GetAll(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _shipmentService.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un envío por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShipmentResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _shipmentService.GetByIdAsync(id, cancellationToken);
        if (item == null)
            return NotFound(new { error = "Envío no encontrado" });

        return Ok(item);
    }

    /// <summary>
    /// Busca un envío por número de tracking.
    /// </summary>
    [HttpGet("by-tracking/{trackingNumber}")]
    public async Task<ActionResult<ShipmentResponse>> ByTracking(
        string trackingNumber,
        CancellationToken cancellationToken = default)
    {
        var item = await _shipmentService.GetByTrackingNumberAsync(trackingNumber, cancellationToken);
        if (item == null)
            return NotFound(new { error = "Envío no encontrado" });

        return Ok(item);
    }

    /// <summary>
    /// Obtiene envíos por estatus.
    /// </summary>
    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<PagedResult<ShipmentResponse>>> ByStatus(
        string status,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ShipmentStatus>(status, out var shipmentStatus))
            return BadRequest(new { error = "Estatus inválido" });

        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var result = await _shipmentService.GetByStatusAsync(tenantId, shipmentStatus, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene envíos del tenant actual.
    /// </summary>
    [HttpGet("current-tenant")]
    public async Task<ActionResult<PagedResult<ShipmentResponse>>> GetByCurrentTenant(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var result = await _shipmentService.GetByTenantAsync(tenantId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene envíos asignados a un chofer.
    /// </summary>
    [HttpGet("by-driver/{driverId:guid}")]
    public async Task<ActionResult<PagedResult<ShipmentResponse>>> ByDriver(
        Guid driverId,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _shipmentService.GetByDriverAsync(driverId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene envíos por ubicación.
    /// </summary>
    [HttpGet("by-location/{locationId:guid}")]
    public async Task<ActionResult<PagedResult<ShipmentResponse>>> ByLocation(
        Guid locationId,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _shipmentService.GetByLocationAsync(locationId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo envío.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ShipmentResponse>> Create(
        [FromBody] CreateShipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _shipmentService.CreateAsync(request, cancellationToken);
        
        if (!result.Success)
            return Conflict(new { error = result.Message });

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            result.Data);
    }

    /// <summary>
    /// Actualiza un envío existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ShipmentResponse>> Update(
        Guid id,
        [FromBody] UpdateShipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _shipmentService.UpdateAsync(id, request, cancellationToken);
        
        if (!result.Success)
        {
            if (result.Message?.Contains("no encontrado") == true)
                return NotFound(new { error = result.Message });
            return Conflict(new { error = result.Message });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Asigna un envío a un chofer y camión.
    /// </summary>
    [HttpPatch("{id:guid}/assign")]
    public async Task<ActionResult<ShipmentResponse>> AssignToDriver(
        Guid id,
        [FromQuery] Guid driverId,
        [FromQuery] Guid truckId,
        CancellationToken cancellationToken = default)
    {
        var result = await _shipmentService.AssignToDriverAsync(id, driverId, truckId, cancellationToken);
        
        if (!result.Success)
            return BadRequest(new { error = result.Message });

        return Ok(result.Data);
    }

    /// <summary>
    /// Actualiza el estatus de un envío.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ShipmentResponse>> UpdateStatus(
        Guid id,
        [FromQuery] string status,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ShipmentStatus>(status, out var newStatus))
            return BadRequest(new { error = "Estatus inválido" });

        var result = await _shipmentService.UpdateStatusAsync(id, newStatus, cancellationToken);
        
        if (!result.Success)
            return BadRequest(new { error = result.Message });

        return Ok(result.Data);
    }

    /// <summary>
    /// Elimina (soft-delete) un envío.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _shipmentService.DeleteAsync(id, cancellationToken);
        
        if (!result.Success)
            return NotFound(new { error = result.Message });

        return NoContent();
    }
}
