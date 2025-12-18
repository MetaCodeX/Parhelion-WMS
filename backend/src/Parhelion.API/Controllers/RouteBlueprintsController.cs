using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Network;
using Parhelion.Application.Interfaces.Services;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para rutas predefinidas.
/// </summary>
[ApiController]
[Route("api/route-blueprints")]
[Authorize]
public class RouteBlueprintsController : ControllerBase
{
    private readonly IRouteService _routeService;

    public RouteBlueprintsController(IRouteService routeService)
    {
        _routeService = routeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _routeService.GetAllAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _routeService.GetByIdAsync(id);
        if (result == null) return NotFound(new { error = "Ruta no encontrada" });
        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<IActionResult> Active([FromQuery] PagedRequest request)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var result = await _routeService.GetActiveAsync(tenantId.Value, request);
        return Ok(result);
    }

    [HttpGet("{id:guid}/steps")]
    public async Task<IActionResult> GetSteps(Guid id)
    {
        var result = await _routeService.GetStepsAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRouteBlueprintRequest request)
    {
        var result = await _routeService.CreateAsync(request);
        if (!result.Success)
            return Conflict(new { error = result.Message });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRouteBlueprintRequest request)
    {
        var result = await _routeService.UpdateAsync(id, request);
        if (!result.Success)
            return (result.Message?.Contains("no encontrad") ?? false)
                ? NotFound(new { error = result.Message }) 
                : BadRequest(new { error = result.Message });
        return Ok(result.Data);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _routeService.DeleteAsync(id);
        if (!result.Success) return NotFound(new { error = result.Message });
        return NoContent();
    }

    private Guid? GetTenantId()
    {
        var claim = User.FindFirst("tenant_id");
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
