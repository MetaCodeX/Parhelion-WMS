using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Enums;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de camiones de la flota.
/// CRUD completo, búsqueda por placa, filtro por tipo y gestión de estatus.
/// </summary>
[ApiController]
[Route("api/trucks")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
public class TrucksController : ControllerBase
{
    private readonly ITruckService _truckService;

    public TrucksController(ITruckService truckService)
    {
        _truckService = truckService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _truckService.GetAllAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _truckService.GetByIdAsync(id);
        if (result == null) return NotFound(new { error = "Camión no encontrado" });
        return Ok(result);
    }

    [HttpGet("available")]
    public async Task<IActionResult> Available([FromQuery] PagedRequest request)
    {
        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var result = await _truckService.GetAvailableAsync(tenantId.Value, request);
        return Ok(result);
    }

    [HttpGet("by-type/{type}")]
    public async Task<IActionResult> ByType(string type, [FromQuery] PagedRequest request)
    {
        if (!Enum.TryParse<TruckType>(type, out var truckType))
            return BadRequest(new { error = "Tipo de camión inválido" });

        var tenantId = GetTenantId();
        if (tenantId == null) return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var result = await _truckService.GetByTypeAsync(tenantId.Value, truckType, request);
        return Ok(result);
    }

    [HttpGet("by-plate/{plate}")]
    public async Task<IActionResult> ByPlate(string plate)
    {
        var result = await _truckService.GetByPlateAsync(plate);
        if (result == null) return NotFound(new { error = "Camión no encontrado" });
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTruckRequest request)
    {
        var result = await _truckService.CreateAsync(request);
        if (!result.Success) 
            return Conflict(new { error = result.Message });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTruckRequest request)
    {
        var result = await _truckService.UpdateAsync(id, request);
        if (!result.Success)
            return (result.Message?.Contains("no encontrado") ?? false)
                ? NotFound(new { error = result.Message }) 
                : BadRequest(new { error = result.Message });
        return Ok(result.Data);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _truckService.DeleteAsync(id);
        if (!result.Success) return NotFound(new { error = result.Message });
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] bool isActive)
    {
        var result = await _truckService.SetActiveStatusAsync(id, isActive);
        if (!result.Success) return NotFound(new { error = result.Message });
        return Ok(result.Data);
    }

    private Guid? GetTenantId()
    {
        var claim = User.FindFirst("tenant_id");
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
