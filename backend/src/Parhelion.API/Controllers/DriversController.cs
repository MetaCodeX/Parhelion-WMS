using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Enums;
using Parhelion.API.Filters;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de choferes.
/// CRUD, filtro por estatus, asignación de camiones y actualización de estado.
/// </summary>
[ApiController]
[Route("api/drivers")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
public class DriversController : ControllerBase
{
    private readonly IDriverService _driverService;

    public DriversController(IDriverService driverService)
    {
        _driverService = driverService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _driverService.GetAllAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _driverService.GetByIdAsync(id);
        if (result == null) return NotFound(new { error = "Chofer no encontrado" });
        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<IActionResult> Active([FromQuery] PagedRequest request)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var result = await _driverService.GetByStatusAsync(tenantId.Value, DriverStatus.Available, request);
        return Ok(result);
    }

    [HttpGet("by-status/{status}")]
    public async Task<IActionResult> ByStatus(string status, [FromQuery] PagedRequest request)
    {
        if (!Enum.TryParse<DriverStatus>(status, out var driverStatus))
            return BadRequest(new { error = "Estatus de chofer inválido" });

        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var result = await _driverService.GetByStatusAsync(tenantId.Value, driverStatus, request);
        return Ok(result);
    }

    /// <summary>
    /// Busca choferes disponibles cercanos a una ubicación.
    /// Autenticación: JWT (Usuario) o Bearer {CallbackToken} / X-Service-Key (n8n).
    /// </summary>
    /// <param name="lat">Latitud central.</param>
    /// <param name="lon">Longitud central.</param>
    /// <param name="radius">Radio en kilómetros (default 50).</param>
    /// <param name="pageNumber">Número de página.</param>
    /// <param name="pageSize">Resultados por página.</param>
    [HttpGet("nearby")]
    [ServiceApiKey] // Permite acceso con X-Service-Key para n8n
    [AllowAnonymous] // Bypass [Authorize] de la clase - ServiceApiKey filter valida
    [ProducesResponseType(typeof(PagedResult<DriverResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNearby(
        [FromQuery] decimal lat, 
        [FromQuery] decimal lon, 
        [FromQuery] double radius = 50,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        // 1. Intentar obtener Tenant del User Claim (JWT)
        var tenantId = GetTenantId();
        
        // 2. Si no hay JWT, obtener del ServiceApiKey (resuelto por el filtro)
        if (tenantId == null && HttpContext.Items.TryGetValue(ServiceApiKeyAttribute.TenantIdKey, out var serviceTenantId))
        {
            tenantId = serviceTenantId as Guid?;
        }

        // 3. Si aún no hay tenant, rechazar
        if (tenantId == null)
        {
            return Unauthorized(new { error = "No se pudo determinar el tenant" });
        }

        var request = new PagedRequest { Page = pageNumber, PageSize = pageSize };
        var result = await _driverService.GetNearbyDriversAsync(lat, lon, radius, tenantId.Value, request);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDriverRequest request)
    {
        var result = await _driverService.CreateAsync(request);
        if (!result.Success) 
            return Conflict(new { error = result.Message });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDriverRequest request)
    {
        var result = await _driverService.UpdateAsync(id, request);
        if (!result.Success)
            return (result.Message?.Contains("no encontrado") ?? false)
                ? NotFound(new { error = result.Message }) 
                : BadRequest(new { error = result.Message });
        return Ok(result.Data);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _driverService.DeleteAsync(id);
        if (!result.Success) return NotFound(new { error = result.Message });
        return NoContent();
    }

    [HttpPatch("{id:guid}/assign-truck")]
    public async Task<IActionResult> AssignTruck(Guid id, [FromBody] AssignTruckRequest request)
    {
        var result = await _driverService.AssignTruckAsync(id, request.TruckId);
        if (!result.Success) return BadRequest(new { error = result.Message });
        return Ok(result.Data);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
    {
        if (!Enum.TryParse<DriverStatus>(status, out var driverStatus))
            return BadRequest(new { error = "Estatus inválido" });

        var result = await _driverService.UpdateStatusAsync(id, driverStatus);
        if (!result.Success) return NotFound(new { error = result.Message });
        return Ok(result.Data);
    }

    private Guid? GetTenantId()
    {
        var claim = User.FindFirst("tenant_id");
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}

public record AssignTruckRequest(Guid TruckId);
