using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de ubicaciones (almacenes, hubs, cross-docks).
/// </summary>
[ApiController]
[Route("api/locations")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public LocationsController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LocationResponse>>> GetAll()
    {
        var items = await _context.Locations
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Code)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LocationResponse>> GetById(Guid id)
    {
        var item = await _context.Locations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Ubicación no encontrada" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("by-type/{type}")]
    public async Task<ActionResult<IEnumerable<LocationResponse>>> ByType(string type)
    {
        if (!Enum.TryParse<LocationType>(type, out var locType))
            return BadRequest(new { error = "Tipo de ubicación inválido" });

        var items = await _context.Locations
            .Where(x => !x.IsDeleted && x.Type == locType)
            .OrderBy(x => x.Code)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<LocationResponse>> Create([FromBody] CreateLocationRequest request)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var existing = await _context.Locations.AnyAsync(x => x.Code == request.Code && !x.IsDeleted);
        if (existing) return Conflict(new { error = $"Ya existe ubicación con código '{request.Code}'" });

        var item = new Location
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = request.Code,
            Name = request.Name,
            Type = Enum.TryParse<LocationType>(request.Type, out var t) ? t : LocationType.Warehouse,
            FullAddress = request.FullAddress,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CanReceive = request.CanReceive,
            CanDispatch = request.CanDispatch,
            IsInternal = request.IsInternal,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Locations.Add(item);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LocationResponse>> Update(Guid id, [FromBody] UpdateLocationRequest request)
    {
        var item = await _context.Locations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Ubicación no encontrada" });

        item.Code = request.Code;
        item.Name = request.Name;
        item.Type = Enum.TryParse<LocationType>(request.Type, out var t) ? t : item.Type;
        item.FullAddress = request.FullAddress;
        item.Latitude = request.Latitude;
        item.Longitude = request.Longitude;
        item.CanReceive = request.CanReceive;
        item.CanDispatch = request.CanDispatch;
        item.IsInternal = request.IsInternal;
        item.IsActive = request.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.Locations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Ubicación no encontrada" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static LocationResponse MapToResponse(Location x) => new(
        x.Id, x.Code, x.Name, x.Type.ToString(), x.FullAddress,
        x.Latitude, x.Longitude, x.CanReceive, x.CanDispatch,
        x.IsInternal, x.IsActive, x.CreatedAt, x.UpdatedAt
    );
}
