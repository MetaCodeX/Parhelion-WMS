using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Network;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para enlaces de red logística.
/// </summary>
[ApiController]
[Route("api/network-links")]
[Authorize]
public class NetworkLinksController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public NetworkLinksController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NetworkLinkResponse>>> GetAll()
    {
        var items = await _context.NetworkLinks
            .Include(x => x.OriginLocation)
            .Include(x => x.DestinationLocation)
            .Where(x => !x.IsDeleted)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NetworkLinkResponse>> GetById(Guid id)
    {
        var item = await _context.NetworkLinks
            .Include(x => x.OriginLocation)
            .Include(x => x.DestinationLocation)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Enlace no encontrado" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("by-location/{locationId:guid}")]
    public async Task<ActionResult<IEnumerable<NetworkLinkResponse>>> ByLocation(Guid locationId)
    {
        var items = await _context.NetworkLinks
            .Include(x => x.OriginLocation)
            .Include(x => x.DestinationLocation)
            .Where(x => !x.IsDeleted && 
                (x.OriginLocationId == locationId || x.DestinationLocationId == locationId))
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<NetworkLinkResponse>> Create([FromBody] CreateNetworkLinkRequest request)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var item = new NetworkLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OriginLocationId = request.OriginLocationId,
            DestinationLocationId = request.DestinationLocationId,
            LinkType = Enum.TryParse<NetworkLinkType>(request.LinkType, out var lt) ? lt : NetworkLinkType.FirstMile,
            TransitTime = request.TransitTime,
            IsBidirectional = request.IsBidirectional,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.NetworkLinks.Add(item);
        await _context.SaveChangesAsync();

        item = await _context.NetworkLinks
            .Include(x => x.OriginLocation)
            .Include(x => x.DestinationLocation)
            .FirstAsync(x => x.Id == item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NetworkLinkResponse>> Update(Guid id, [FromBody] UpdateNetworkLinkRequest request)
    {
        var item = await _context.NetworkLinks
            .Include(x => x.OriginLocation)
            .Include(x => x.DestinationLocation)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Enlace no encontrado" });

        item.LinkType = Enum.TryParse<NetworkLinkType>(request.LinkType, out var lt) ? lt : item.LinkType;
        item.TransitTime = request.TransitTime;
        item.IsBidirectional = request.IsBidirectional;
        item.IsActive = request.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.NetworkLinks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Enlace no encontrado" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static NetworkLinkResponse MapToResponse(NetworkLink x) => new(
        x.Id, x.OriginLocationId, x.OriginLocation?.Name ?? "",
        x.DestinationLocationId, x.DestinationLocation?.Name ?? "",
        x.LinkType.ToString(), x.TransitTime, x.IsBidirectional, x.IsActive,
        x.CreatedAt, x.UpdatedAt
    );
}
