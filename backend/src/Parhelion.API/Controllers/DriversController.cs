using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de choferes.
/// </summary>
[ApiController]
[Route("api/drivers")]
[Authorize]
public class DriversController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public DriversController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DriverResponse>>> GetAll()
    {
        var items = await _context.Drivers
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.DefaultTruck)
            .Include(x => x.CurrentTruck)
            .Where(x => !x.IsDeleted)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DriverResponse>> GetById(Guid id)
    {
        var item = await _context.Drivers
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.DefaultTruck)
            .Include(x => x.CurrentTruck)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Chofer no encontrado" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<DriverResponse>>> Active()
    {
        var items = await _context.Drivers
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.DefaultTruck)
            .Include(x => x.CurrentTruck)
            .Where(x => !x.IsDeleted && x.Status == DriverStatus.Available)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<IEnumerable<DriverResponse>>> ByStatus(string status)
    {
        if (!Enum.TryParse<DriverStatus>(status, out var driverStatus))
            return BadRequest(new { error = "Estatus de chofer inválido" });

        var items = await _context.Drivers
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.DefaultTruck)
            .Include(x => x.CurrentTruck)
            .Where(x => !x.IsDeleted && x.Status == driverStatus)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<DriverResponse>> Create([FromBody] CreateDriverRequest request)
    {
        var item = new Driver
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            LicenseNumber = request.LicenseNumber,
            LicenseType = request.LicenseType,
            LicenseExpiration = request.LicenseExpiration,
            DefaultTruckId = request.DefaultTruckId,
            Status = Enum.TryParse<DriverStatus>(request.Status, out var s) ? s : DriverStatus.Available,
            CreatedAt = DateTime.UtcNow
        };

        _context.Drivers.Add(item);
        await _context.SaveChangesAsync();

        item = await _context.Drivers
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.DefaultTruck)
            .Include(x => x.CurrentTruck)
            .FirstAsync(x => x.Id == item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DriverResponse>> Update(Guid id, [FromBody] UpdateDriverRequest request)
    {
        var item = await _context.Drivers
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.DefaultTruck)
            .Include(x => x.CurrentTruck)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Chofer no encontrado" });

        item.LicenseNumber = request.LicenseNumber;
        item.LicenseType = request.LicenseType;
        item.LicenseExpiration = request.LicenseExpiration;
        item.DefaultTruckId = request.DefaultTruckId;
        item.CurrentTruckId = request.CurrentTruckId;
        item.Status = Enum.TryParse<DriverStatus>(request.Status, out var s) ? s : item.Status;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.Drivers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Chofer no encontrado" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static DriverResponse MapToResponse(Driver x) => new(
        x.Id, x.EmployeeId, x.Employee?.User?.FullName ?? "",
        x.LicenseNumber, x.LicenseType, x.LicenseExpiration,
        x.DefaultTruckId, x.DefaultTruck?.Plate,
        x.CurrentTruckId, x.CurrentTruck?.Plate,
        x.Status.ToString(), x.CreatedAt, x.UpdatedAt
    );
}
