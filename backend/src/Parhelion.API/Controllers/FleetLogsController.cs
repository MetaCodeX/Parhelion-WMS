using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Application.Interfaces.Services;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para bitácora de flotilla (cambios de camión).
/// Los logs son inmutables - solo se crean y consultan.
/// </summary>
[ApiController]
[Route("api/fleet-logs")]
[Authorize]
public class FleetLogsController : ControllerBase
{
    private readonly IFleetLogService _fleetLogService;

    public FleetLogsController(IFleetLogService fleetLogService)
    {
        _fleetLogService = fleetLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _fleetLogService.GetAllAsync(request);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _fleetLogService.GetByIdAsync(id);
        if (result == null) return NotFound(new { error = "Log no encontrado" });
        return Ok(result);
    }

    [HttpGet("by-driver/{driverId:guid}")]
    public async Task<IActionResult> ByDriver(Guid driverId, [FromQuery] PagedRequest request)
    {
        var result = await _fleetLogService.GetByDriverAsync(driverId, request);
        return Ok(result);
    }

    [HttpGet("by-truck/{truckId:guid}")]
    public async Task<IActionResult> ByTruck(Guid truckId, [FromQuery] PagedRequest request)
    {
        var result = await _fleetLogService.GetByTruckAsync(truckId, request);
        return Ok(result);
    }

    [HttpPost("start-usage")]
    public async Task<IActionResult> StartUsage([FromBody] StartUsageRequest request)
    {
        var result = await _fleetLogService.StartUsageAsync(request.DriverId, request.TruckId);
        if (!result.Success) return BadRequest(new { error = result.Message });
        return Ok(result.Data);
    }

    [HttpPost("end-usage")]
    public async Task<IActionResult> EndUsage([FromBody] EndUsageRequest request)
    {
        // Get active log for driver and end it
        var activeLog = await _fleetLogService.GetActiveLogForDriverAsync(request.DriverId);
        if (activeLog == null) return NotFound(new { error = "No hay uso activo para este chofer" });

        var result = await _fleetLogService.EndUsageAsync(activeLog.Id, request.EndOdometer);
        if (!result.Success) return BadRequest(new { error = result.Message });
        return Ok(result.Data);
    }

    // No PUT/DELETE - logs are immutable
}

public record StartUsageRequest(Guid DriverId, Guid TruckId);
public record EndUsageRequest(Guid DriverId, decimal? EndOdometer = null);
