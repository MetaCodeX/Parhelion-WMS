using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Domain.Entities;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para operadores de almacén.
/// </summary>
[ApiController]
[Route("api/warehouse-operators")]
[Authorize]
public class WarehouseOperatorsController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public WarehouseOperatorsController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WarehouseOperatorResponse>>> GetAll()
    {
        var items = await _context.WarehouseOperators
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.AssignedLocation)
            .Include(x => x.PrimaryZone)
            .Where(x => !x.IsDeleted)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WarehouseOperatorResponse>> GetById(Guid id)
    {
        var item = await _context.WarehouseOperators
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.AssignedLocation)
            .Include(x => x.PrimaryZone)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Operador no encontrado" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("by-location/{locationId:guid}")]
    public async Task<ActionResult<IEnumerable<WarehouseOperatorResponse>>> ByLocation(Guid locationId)
    {
        var items = await _context.WarehouseOperators
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.AssignedLocation)
            .Include(x => x.PrimaryZone)
            .Where(x => !x.IsDeleted && x.AssignedLocationId == locationId)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseOperatorResponse>> Create([FromBody] CreateWarehouseOperatorRequest request)
    {
        var item = new WarehouseOperator
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            AssignedLocationId = request.AssignedLocationId,
            PrimaryZoneId = request.PrimaryZoneId,
            CreatedAt = DateTime.UtcNow
        };

        _context.WarehouseOperators.Add(item);
        await _context.SaveChangesAsync();

        item = await _context.WarehouseOperators
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.AssignedLocation)
            .Include(x => x.PrimaryZone)
            .FirstAsync(x => x.Id == item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WarehouseOperatorResponse>> Update(Guid id, [FromBody] UpdateWarehouseOperatorRequest request)
    {
        var item = await _context.WarehouseOperators
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.AssignedLocation)
            .Include(x => x.PrimaryZone)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Operador no encontrado" });

        item.AssignedLocationId = request.AssignedLocationId;
        item.PrimaryZoneId = request.PrimaryZoneId;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.WarehouseOperators.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Operador no encontrado" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static WarehouseOperatorResponse MapToResponse(WarehouseOperator x) => new(
        x.Id, x.EmployeeId, x.Employee?.User?.FullName ?? "",
        x.AssignedLocationId, x.AssignedLocation?.Name ?? "",
        x.PrimaryZoneId, x.PrimaryZone?.Name,
        x.CreatedAt, x.UpdatedAt
    );
}
