using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Enums;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de ubicaciones (almacenes, hubs, cross-docks).
/// </summary>
[ApiController]
[Route("api/locations")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _locationService.GetAllAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _locationService.GetByIdAsync(id);
        if (result == null) return NotFound(new { error = "Ubicación no encontrada" });
        return Ok(result);
    }

    [HttpGet("by-type/{type}")]
    public async Task<IActionResult> ByType(string type, [FromQuery] PagedRequest request)
    {
        if (!Enum.TryParse<LocationType>(type, out var locType))
            return BadRequest(new { error = "Tipo de ubicación inválido" });

        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var result = await _locationService.GetByTypeAsync(tenantId.Value, locType, request);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string name, [FromQuery] PagedRequest request)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var result = await _locationService.SearchByNameAsync(tenantId.Value, name, request);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLocationRequest request)
    {
        var result = await _locationService.CreateAsync(request);
        if (!result.Success)
            return Conflict(new { error = result.Message });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLocationRequest request)
    {
        var result = await _locationService.UpdateAsync(id, request);
        if (!result.Success)
            return (result.Message?.Contains("no encontrad") ?? false)
                ? NotFound(new { error = result.Message }) 
                : BadRequest(new { error = result.Message });
        return Ok(result.Data);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _locationService.DeleteAsync(id);
        if (!result.Success) return NotFound(new { error = result.Message });
        return NoContent();
    }

    private Guid? GetTenantId()
    {
        var claim = User.FindFirst("tenant_id");
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
