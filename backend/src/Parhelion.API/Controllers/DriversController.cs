using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Enums;

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
