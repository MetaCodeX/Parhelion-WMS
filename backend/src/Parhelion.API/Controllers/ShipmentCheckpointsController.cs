using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para checkpoints de envío (trazabilidad).
/// Los checkpoints son inmutables.
/// </summary>
[ApiController]
[Route("api/shipment-checkpoints")]
[Authorize]
public class ShipmentCheckpointsController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public ShipmentCheckpointsController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShipmentCheckpointResponse>>> GetAll()
    {
        var items = await GetCheckpointWithIncludes()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.Timestamp)
            .Take(100)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShipmentCheckpointResponse>> GetById(Guid id)
    {
        var item = await GetCheckpointWithIncludes().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Checkpoint no encontrado" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("by-shipment/{shipmentId:guid}")]
    public async Task<ActionResult<IEnumerable<ShipmentCheckpointResponse>>> ByShipment(Guid shipmentId)
    {
        var items = await GetCheckpointWithIncludes()
            .Where(x => !x.IsDeleted && x.ShipmentId == shipmentId)
            .OrderByDescending(x => x.Timestamp)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<ShipmentCheckpointResponse>> Create([FromBody] CreateShipmentCheckpointRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new { error = "No se pudo determinar el usuario" });

        var item = new ShipmentCheckpoint
        {
            Id = Guid.NewGuid(),
            ShipmentId = request.ShipmentId,
            LocationId = request.LocationId,
            StatusCode = Enum.TryParse<CheckpointStatus>(request.StatusCode, out var s) ? s : CheckpointStatus.Loaded,
            Remarks = request.Remarks,
            Timestamp = DateTime.UtcNow,
            CreatedByUserId = userId,
            HandledByDriverId = request.HandledByDriverId,
            LoadedOntoTruckId = request.LoadedOntoTruckId,
            ActionType = request.ActionType,
            PreviousCustodian = request.PreviousCustodian,
            NewCustodian = request.NewCustodian,
            HandledByWarehouseOperatorId = request.HandledByWarehouseOperatorId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CreatedAt = DateTime.UtcNow
        };

        _context.ShipmentCheckpoints.Add(item);
        await _context.SaveChangesAsync();

        item = await GetCheckpointWithIncludes().FirstAsync(x => x.Id == item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    // No PUT/DELETE - checkpoints are immutable

    private IQueryable<ShipmentCheckpoint> GetCheckpointWithIncludes() => _context.ShipmentCheckpoints
        .Include(x => x.Location)
        .Include(x => x.CreatedBy)
        .Include(x => x.HandledByDriver).ThenInclude(d => d!.Employee).ThenInclude(e => e.User)
        .Include(x => x.LoadedOntoTruck);

    private static ShipmentCheckpointResponse MapToResponse(ShipmentCheckpoint x) => new(
        x.Id, x.ShipmentId, x.LocationId, x.Location?.Name,
        x.StatusCode.ToString(), x.Remarks, x.Timestamp,
        x.CreatedByUserId, x.CreatedBy?.FullName ?? "",
        x.HandledByDriverId, x.HandledByDriver?.Employee?.User?.FullName,
        x.LoadedOntoTruckId, x.LoadedOntoTruck?.Plate,
        x.ActionType, x.PreviousCustodian, x.NewCustodian,
        x.Latitude, x.Longitude, x.CreatedAt
    );
}
