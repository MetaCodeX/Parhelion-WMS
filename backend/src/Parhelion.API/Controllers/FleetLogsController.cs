using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Data;

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
    private readonly ParhelionDbContext _context;

    public FleetLogsController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FleetLogResponse>>> GetAll()
    {
        var items = await _context.FleetLogs
            .Include(x => x.Driver).ThenInclude(d => d.Employee).ThenInclude(e => e.User)
            .Include(x => x.OldTruck)
            .Include(x => x.NewTruck)
            .Include(x => x.CreatedBy)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.Timestamp)
            .Take(100)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FleetLogResponse>> GetById(Guid id)
    {
        var item = await _context.FleetLogs
            .Include(x => x.Driver).ThenInclude(d => d.Employee).ThenInclude(e => e.User)
            .Include(x => x.OldTruck)
            .Include(x => x.NewTruck)
            .Include(x => x.CreatedBy)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Log no encontrado" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("by-driver/{driverId:guid}")]
    public async Task<ActionResult<IEnumerable<FleetLogResponse>>> ByDriver(Guid driverId)
    {
        var items = await _context.FleetLogs
            .Include(x => x.Driver).ThenInclude(d => d.Employee).ThenInclude(e => e.User)
            .Include(x => x.OldTruck)
            .Include(x => x.NewTruck)
            .Include(x => x.CreatedBy)
            .Where(x => !x.IsDeleted && x.DriverId == driverId)
            .OrderByDescending(x => x.Timestamp)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("by-truck/{truckId:guid}")]
    public async Task<ActionResult<IEnumerable<FleetLogResponse>>> ByTruck(Guid truckId)
    {
        var items = await _context.FleetLogs
            .Include(x => x.Driver).ThenInclude(d => d.Employee).ThenInclude(e => e.User)
            .Include(x => x.OldTruck)
            .Include(x => x.NewTruck)
            .Include(x => x.CreatedBy)
            .Where(x => !x.IsDeleted && (x.OldTruckId == truckId || x.NewTruckId == truckId))
            .OrderByDescending(x => x.Timestamp)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<FleetLogResponse>> Create([FromBody] CreateFleetLogRequest request)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new { error = "No se pudo determinar el usuario" });

        var item = new FleetLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DriverId = request.DriverId,
            OldTruckId = request.OldTruckId,
            NewTruckId = request.NewTruckId,
            Reason = Enum.TryParse<FleetLogReason>(request.Reason, out var r) ? r : FleetLogReason.Reassignment,
            Timestamp = DateTime.UtcNow,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.FleetLogs.Add(item);

        // Update driver's current truck
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == request.DriverId);
        if (driver != null)
        {
            driver.CurrentTruckId = request.NewTruckId;
        }

        await _context.SaveChangesAsync();

        item = await _context.FleetLogs
            .Include(x => x.Driver).ThenInclude(d => d.Employee).ThenInclude(e => e.User)
            .Include(x => x.OldTruck)
            .Include(x => x.NewTruck)
            .Include(x => x.CreatedBy)
            .FirstAsync(x => x.Id == item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    // No PUT/DELETE - logs are immutable

    private static FleetLogResponse MapToResponse(FleetLog x) => new(
        x.Id, x.DriverId, x.Driver?.Employee?.User?.FullName ?? "",
        x.OldTruckId, x.OldTruck?.Plate,
        x.NewTruckId, x.NewTruck?.Plate ?? "",
        x.Reason.ToString(), x.Timestamp,
        x.CreatedByUserId, x.CreatedBy?.FullName ?? "",
        x.CreatedAt
    );
}
