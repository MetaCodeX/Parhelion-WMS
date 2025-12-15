using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de envíos.
/// </summary>
[ApiController]
[Route("api/shipments")]
[Authorize]
public class ShipmentsController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public ShipmentsController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShipmentResponse>>> GetAll()
    {
        var items = await _context.Shipments
            .Include(x => x.OriginLocation)
            .Include(x => x.DestinationLocation)
            .Include(x => x.Sender)
            .Include(x => x.RecipientClient)
            .Include(x => x.Truck)
            .Include(x => x.Driver).ThenInclude(d => d!.Employee).ThenInclude(e => e.User)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShipmentResponse>> GetById(Guid id)
    {
        var item = await GetShipmentWithIncludes().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Envío no encontrado" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("by-tracking/{trackingNumber}")]
    public async Task<ActionResult<ShipmentResponse>> ByTracking(string trackingNumber)
    {
        var item = await GetShipmentWithIncludes()
            .FirstOrDefaultAsync(x => x.TrackingNumber == trackingNumber && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Envío no encontrado" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<IEnumerable<ShipmentResponse>>> ByStatus(string status)
    {
        if (!Enum.TryParse<ShipmentStatus>(status, out var shipmentStatus))
            return BadRequest(new { error = "Estatus inválido" });

        var items = await GetShipmentWithIncludes()
            .Where(x => !x.IsDeleted && x.Status == shipmentStatus)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<ShipmentResponse>> Create([FromBody] CreateShipmentRequest request)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var trackingNumber = GenerateTrackingNumber();
        var item = new Domain.Entities.Shipment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrackingNumber = trackingNumber,
            QrCodeData = $"PAR:{trackingNumber}",
            OriginLocationId = request.OriginLocationId,
            DestinationLocationId = request.DestinationLocationId,
            SenderId = request.SenderId,
            RecipientClientId = request.RecipientClientId,
            RecipientName = request.RecipientName,
            RecipientPhone = request.RecipientPhone,
            TotalWeightKg = request.TotalWeightKg,
            TotalVolumeM3 = request.TotalVolumeM3,
            DeclaredValue = request.DeclaredValue,
            SatMerchandiseCode = request.SatMerchandiseCode,
            DeliveryInstructions = request.DeliveryInstructions,
            Priority = Enum.TryParse<ShipmentPriority>(request.Priority, out var p) ? p : ShipmentPriority.Normal,
            Status = ShipmentStatus.PendingApproval,
            CreatedAt = DateTime.UtcNow
        };

        _context.Shipments.Add(item);
        await _context.SaveChangesAsync();

        item = await GetShipmentWithIncludes().FirstAsync(x => x.Id == item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ShipmentResponse>> Update(Guid id, [FromBody] UpdateShipmentRequest request)
    {
        var item = await GetShipmentWithIncludes().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Envío no encontrado" });

        item.AssignedRouteId = request.AssignedRouteId;
        item.CurrentStepOrder = request.CurrentStepOrder;
        item.DeliveryInstructions = request.DeliveryInstructions;
        item.Priority = Enum.TryParse<ShipmentPriority>(request.Priority, out var p) ? p : item.Priority;
        item.Status = Enum.TryParse<ShipmentStatus>(request.Status, out var s) ? s : item.Status;
        item.TruckId = request.TruckId;
        item.DriverId = request.DriverId;
        item.WasQrScanned = request.WasQrScanned;
        item.IsDelayed = request.IsDelayed;
        item.ScheduledDeparture = request.ScheduledDeparture;
        item.PickupWindowStart = request.PickupWindowStart;
        item.PickupWindowEnd = request.PickupWindowEnd;
        item.EstimatedArrival = request.EstimatedArrival;
        item.AssignedAt = request.AssignedAt;
        item.DeliveredAt = request.DeliveredAt;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.Shipments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Envío no encontrado" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<Domain.Entities.Shipment> GetShipmentWithIncludes() => _context.Shipments
        .Include(x => x.OriginLocation)
        .Include(x => x.DestinationLocation)
        .Include(x => x.Sender)
        .Include(x => x.RecipientClient)
        .Include(x => x.Truck)
        .Include(x => x.Driver).ThenInclude(d => d!.Employee).ThenInclude(e => e.User);

    private static string GenerateTrackingNumber()
    {
        var random = new Random();
        return $"PAR-{random.Next(100000, 999999)}";
    }

    private static ShipmentResponse MapToResponse(Domain.Entities.Shipment x) => new(
        x.Id, x.TrackingNumber, x.QrCodeData,
        x.OriginLocationId, x.OriginLocation?.Name ?? "",
        x.DestinationLocationId, x.DestinationLocation?.Name ?? "",
        x.SenderId, x.Sender?.CompanyName,
        x.RecipientClientId, x.RecipientClient?.CompanyName,
        x.RecipientName, x.RecipientPhone,
        x.TotalWeightKg, x.TotalVolumeM3, x.DeclaredValue,
        x.SatMerchandiseCode, x.DeliveryInstructions,
        x.Priority.ToString(), x.Status.ToString(),
        x.TruckId, x.Truck?.Plate,
        x.DriverId, x.Driver?.Employee?.User?.FullName,
        x.WasQrScanned, x.IsDelayed,
        x.ScheduledDeparture, x.EstimatedArrival, x.DeliveredAt,
        x.CreatedAt, x.UpdatedAt
    );
}
