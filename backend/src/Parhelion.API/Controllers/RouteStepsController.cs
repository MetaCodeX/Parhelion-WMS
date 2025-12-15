using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Network;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para pasos de ruta.
/// </summary>
[ApiController]
[Route("api/route-steps")]
[Authorize]
public class RouteStepsController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public RouteStepsController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RouteStepResponse>>> GetAll()
    {
        var items = await _context.RouteSteps
            .Include(x => x.RouteBlueprint)
            .Include(x => x.Location)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.RouteBlueprintId).ThenBy(x => x.StepOrder)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RouteStepResponse>> GetById(Guid id)
    {
        var item = await _context.RouteSteps
            .Include(x => x.RouteBlueprint)
            .Include(x => x.Location)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Paso no encontrado" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("by-route/{routeId:guid}")]
    public async Task<ActionResult<IEnumerable<RouteStepResponse>>> ByRoute(Guid routeId)
    {
        var items = await _context.RouteSteps
            .Include(x => x.RouteBlueprint)
            .Include(x => x.Location)
            .Where(x => !x.IsDeleted && x.RouteBlueprintId == routeId)
            .OrderBy(x => x.StepOrder)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<RouteStepResponse>> Create([FromBody] CreateRouteStepRequest request)
    {
        var item = new RouteStep
        {
            Id = Guid.NewGuid(),
            RouteBlueprintId = request.RouteBlueprintId,
            LocationId = request.LocationId,
            StepOrder = request.StepOrder,
            StandardTransitTime = request.StandardTransitTime,
            StepType = Enum.TryParse<RouteStepType>(request.StepType, out var st) ? st : RouteStepType.Origin,
            CreatedAt = DateTime.UtcNow
        };

        _context.RouteSteps.Add(item);

        // Update route totals
        var route = await _context.RouteBlueprints.FirstOrDefaultAsync(r => r.Id == request.RouteBlueprintId);
        if (route != null)
        {
            route.TotalSteps++;
            route.TotalTransitTime += request.StandardTransitTime;
        }

        await _context.SaveChangesAsync();

        item = await _context.RouteSteps
            .Include(x => x.RouteBlueprint)
            .Include(x => x.Location)
            .FirstAsync(x => x.Id == item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RouteStepResponse>> Update(Guid id, [FromBody] UpdateRouteStepRequest request)
    {
        var item = await _context.RouteSteps
            .Include(x => x.RouteBlueprint)
            .Include(x => x.Location)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Paso no encontrado" });

        item.LocationId = request.LocationId;
        item.StepOrder = request.StepOrder;
        item.StandardTransitTime = request.StandardTransitTime;
        item.StepType = Enum.TryParse<RouteStepType>(request.StepType, out var st) ? st : item.StepType;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.RouteSteps.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Paso no encontrado" });

        // Update route totals
        var route = await _context.RouteBlueprints.FirstOrDefaultAsync(r => r.Id == item.RouteBlueprintId);
        if (route != null)
        {
            route.TotalSteps--;
            route.TotalTransitTime -= item.StandardTransitTime;
        }

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static RouteStepResponse MapToResponse(RouteStep x) => new(
        x.Id, x.RouteBlueprintId, x.RouteBlueprint?.Name ?? "",
        x.LocationId, x.Location?.Name ?? "",
        x.StepOrder, x.StandardTransitTime, x.StepType.ToString(),
        x.CreatedAt, x.UpdatedAt
    );
}
