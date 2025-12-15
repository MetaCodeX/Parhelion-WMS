using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para zonas de bodega.
/// </summary>
[ApiController]
[Route("api/warehouse-zones")]
[Authorize]
public class WarehouseZonesController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public WarehouseZonesController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WarehouseZoneResponse>>> GetAll()
    {
        var items = await _context.WarehouseZones
            .Include(x => x.Location)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Code)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WarehouseZoneResponse>> GetById(Guid id)
    {
        var item = await _context.WarehouseZones
            .Include(x => x.Location)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Zona no encontrada" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("by-location/{locationId:guid}")]
    public async Task<ActionResult<IEnumerable<WarehouseZoneResponse>>> ByLocation(Guid locationId)
    {
        var items = await _context.WarehouseZones
            .Include(x => x.Location)
            .Where(x => !x.IsDeleted && x.LocationId == locationId)
            .OrderBy(x => x.Code)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseZoneResponse>> Create([FromBody] CreateWarehouseZoneRequest request)
    {
        var item = new WarehouseZone
        {
            Id = Guid.NewGuid(),
            LocationId = request.LocationId,
            Code = request.Code,
            Name = request.Name,
            Type = Enum.TryParse<WarehouseZoneType>(request.Type, out var t) ? t : WarehouseZoneType.Storage,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.WarehouseZones.Add(item);
        await _context.SaveChangesAsync();

        item = await _context.WarehouseZones.Include(x => x.Location).FirstAsync(x => x.Id == item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WarehouseZoneResponse>> Update(Guid id, [FromBody] UpdateWarehouseZoneRequest request)
    {
        var item = await _context.WarehouseZones.Include(x => x.Location).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Zona no encontrada" });

        item.Code = request.Code;
        item.Name = request.Name;
        item.Type = Enum.TryParse<WarehouseZoneType>(request.Type, out var t) ? t : item.Type;
        item.IsActive = request.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.WarehouseZones.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Zona no encontrada" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static WarehouseZoneResponse MapToResponse(WarehouseZone x) => new(
        x.Id, x.LocationId, x.Location?.Name ?? "", x.Code, x.Name,
        x.Type.ToString(), x.IsActive, x.CreatedAt, x.UpdatedAt
    );
}
