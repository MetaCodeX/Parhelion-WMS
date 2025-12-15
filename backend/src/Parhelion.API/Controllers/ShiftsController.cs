using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Domain.Entities;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para turnos de trabajo.
/// </summary>
[ApiController]
[Route("api/shifts")]
[Authorize]
public class ShiftsController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public ShiftsController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShiftResponse>>> GetAll()
    {
        var items = await _context.Shifts
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShiftResponse>> GetById(Guid id)
    {
        var item = await _context.Shifts.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Turno no encontrado" });
        return Ok(MapToResponse(item));
    }

    [HttpPost]
    public async Task<ActionResult<ShiftResponse>> Create([FromBody] CreateShiftRequest request)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var item = new Shift
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            DaysOfWeek = request.DaysOfWeek,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Shifts.Add(item);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ShiftResponse>> Update(Guid id, [FromBody] UpdateShiftRequest request)
    {
        var item = await _context.Shifts.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Turno no encontrado" });

        item.Name = request.Name;
        item.StartTime = request.StartTime;
        item.EndTime = request.EndTime;
        item.DaysOfWeek = request.DaysOfWeek;
        item.IsActive = request.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.Shifts.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Turno no encontrado" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static ShiftResponse MapToResponse(Shift x) => new(
        x.Id, x.Name, x.StartTime, x.EndTime, x.DaysOfWeek,
        x.IsActive, x.CreatedAt, x.UpdatedAt
    );
}
