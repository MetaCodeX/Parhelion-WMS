using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Network;
using Parhelion.Domain.Entities;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para rutas predefinidas.
/// </summary>
[ApiController]
[Route("api/route-blueprints")]
[Authorize]
public class RouteBlueprintsController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public RouteBlueprintsController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RouteBlueprintResponse>>> GetAll()
    {
        var items = await _context.RouteBlueprints
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RouteBlueprintResponse>> GetById(Guid id)
    {
        var item = await _context.RouteBlueprints.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Ruta no encontrada" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<RouteBlueprintResponse>>> Active()
    {
        var items = await _context.RouteBlueprints
            .Where(x => !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<RouteBlueprintResponse>> Create([FromBody] CreateRouteBlueprintRequest request)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var item = new RouteBlueprint
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            TotalSteps = 0,
            TotalTransitTime = TimeSpan.Zero,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.RouteBlueprints.Add(item);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RouteBlueprintResponse>> Update(Guid id, [FromBody] UpdateRouteBlueprintRequest request)
    {
        var item = await _context.RouteBlueprints.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Ruta no encontrada" });

        item.Name = request.Name;
        item.Description = request.Description;
        item.TotalSteps = request.TotalSteps;
        item.TotalTransitTime = request.TotalTransitTime;
        item.IsActive = request.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.RouteBlueprints.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Ruta no encontrada" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static RouteBlueprintResponse MapToResponse(RouteBlueprint x) => new(
        x.Id, x.Name, x.Description, x.TotalSteps, x.TotalTransitTime,
        x.IsActive, x.CreatedAt, x.UpdatedAt
    );
}
