using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de camiones.
/// </summary>
[ApiController]
[Route("api/trucks")]
[Authorize]
public class TrucksController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public TrucksController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TruckResponse>>> GetAll()
    {
        var items = await _context.Trucks
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Plate)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TruckResponse>> GetById(Guid id)
    {
        var item = await _context.Trucks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Camión no encontrado" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<TruckResponse>>> Available()
    {
        var items = await _context.Trucks
            .Where(x => !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.Plate)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("by-type/{type}")]
    public async Task<ActionResult<IEnumerable<TruckResponse>>> ByType(string type)
    {
        if (!Enum.TryParse<TruckType>(type, out var truckType))
            return BadRequest(new { error = "Tipo de camión inválido" });

        var items = await _context.Trucks
            .Where(x => !x.IsDeleted && x.Type == truckType)
            .OrderBy(x => x.Plate)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<TruckResponse>> Create([FromBody] CreateTruckRequest request)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var existing = await _context.Trucks.AnyAsync(x => x.Plate == request.Plate && !x.IsDeleted);
        if (existing) return Conflict(new { error = $"Ya existe camión con placa '{request.Plate}'" });

        var item = new Truck
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Plate = request.Plate,
            Model = request.Model,
            Type = Enum.TryParse<TruckType>(request.Type, out var t) ? t : TruckType.DryBox,
            MaxCapacityKg = request.MaxCapacityKg,
            MaxVolumeM3 = request.MaxVolumeM3,
            IsActive = true,
            Vin = request.Vin,
            EngineNumber = request.EngineNumber,
            Year = request.Year,
            Color = request.Color,
            InsurancePolicy = request.InsurancePolicy,
            InsuranceExpiration = request.InsuranceExpiration,
            VerificationNumber = request.VerificationNumber,
            VerificationExpiration = request.VerificationExpiration,
            CreatedAt = DateTime.UtcNow
        };

        _context.Trucks.Add(item);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TruckResponse>> Update(Guid id, [FromBody] UpdateTruckRequest request)
    {
        var item = await _context.Trucks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Camión no encontrado" });

        item.Plate = request.Plate;
        item.Model = request.Model;
        item.Type = Enum.TryParse<TruckType>(request.Type, out var t) ? t : item.Type;
        item.MaxCapacityKg = request.MaxCapacityKg;
        item.MaxVolumeM3 = request.MaxVolumeM3;
        item.IsActive = request.IsActive;
        item.Vin = request.Vin;
        item.EngineNumber = request.EngineNumber;
        item.Year = request.Year;
        item.Color = request.Color;
        item.InsurancePolicy = request.InsurancePolicy;
        item.InsuranceExpiration = request.InsuranceExpiration;
        item.VerificationNumber = request.VerificationNumber;
        item.VerificationExpiration = request.VerificationExpiration;
        item.LastMaintenanceDate = request.LastMaintenanceDate;
        item.NextMaintenanceDate = request.NextMaintenanceDate;
        item.CurrentOdometerKm = request.CurrentOdometerKm;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.Trucks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Camión no encontrado" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static TruckResponse MapToResponse(Truck x) => new(
        x.Id, x.Plate, x.Model, x.Type.ToString(), x.MaxCapacityKg, x.MaxVolumeM3, x.IsActive,
        x.Vin, x.EngineNumber, x.Year, x.Color, x.InsurancePolicy, x.InsuranceExpiration,
        x.VerificationNumber, x.VerificationExpiration, x.LastMaintenanceDate,
        x.NextMaintenanceDate, x.CurrentOdometerKm, x.CreatedAt, x.UpdatedAt
    );
}
