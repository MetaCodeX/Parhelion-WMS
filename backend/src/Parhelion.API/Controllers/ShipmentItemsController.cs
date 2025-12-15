using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para items de envío.
/// </summary>
[ApiController]
[Route("api/shipment-items")]
[Authorize]
public class ShipmentItemsController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public ShipmentItemsController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShipmentItemResponse>>> GetAll()
    {
        var items = await _context.ShipmentItems
            .Include(x => x.Product)
            .Where(x => !x.IsDeleted)
            .Take(100)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShipmentItemResponse>> GetById(Guid id)
    {
        var item = await _context.ShipmentItems
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Item no encontrado" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("by-shipment/{shipmentId:guid}")]
    public async Task<ActionResult<IEnumerable<ShipmentItemResponse>>> ByShipment(Guid shipmentId)
    {
        var items = await _context.ShipmentItems
            .Include(x => x.Product)
            .Where(x => !x.IsDeleted && x.ShipmentId == shipmentId)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<ShipmentItemResponse>> Create([FromBody] CreateShipmentItemRequest request)
    {
        var item = new ShipmentItem
        {
            Id = Guid.NewGuid(),
            ShipmentId = request.ShipmentId,
            ProductId = request.ProductId,
            Sku = request.Sku,
            Description = request.Description,
            PackagingType = Enum.TryParse<PackagingType>(request.PackagingType, out var pt) ? pt : PackagingType.Box,
            Quantity = request.Quantity,
            WeightKg = request.WeightKg,
            WidthCm = request.WidthCm,
            HeightCm = request.HeightCm,
            LengthCm = request.LengthCm,
            DeclaredValue = request.DeclaredValue,
            IsFragile = request.IsFragile,
            IsHazardous = request.IsHazardous,
            RequiresRefrigeration = request.RequiresRefrigeration,
            StackingInstructions = request.StackingInstructions,
            CreatedAt = DateTime.UtcNow
        };

        _context.ShipmentItems.Add(item);
        await _context.SaveChangesAsync();

        item = await _context.ShipmentItems.Include(x => x.Product).FirstAsync(x => x.Id == item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ShipmentItemResponse>> Update(Guid id, [FromBody] UpdateShipmentItemRequest request)
    {
        var item = await _context.ShipmentItems.Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Item no encontrado" });

        item.Sku = request.Sku;
        item.Description = request.Description;
        item.PackagingType = Enum.TryParse<PackagingType>(request.PackagingType, out var pt) ? pt : item.PackagingType;
        item.Quantity = request.Quantity;
        item.WeightKg = request.WeightKg;
        item.WidthCm = request.WidthCm;
        item.HeightCm = request.HeightCm;
        item.LengthCm = request.LengthCm;
        item.DeclaredValue = request.DeclaredValue;
        item.IsFragile = request.IsFragile;
        item.IsHazardous = request.IsHazardous;
        item.RequiresRefrigeration = request.RequiresRefrigeration;
        item.StackingInstructions = request.StackingInstructions;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.ShipmentItems.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Item no encontrado" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static ShipmentItemResponse MapToResponse(ShipmentItem x) => new(
        x.Id, x.ShipmentId, x.ProductId, x.Product?.Name,
        x.Sku, x.Description, x.PackagingType.ToString(),
        x.Quantity, x.WeightKg, x.WidthCm, x.HeightCm, x.LengthCm,
        x.VolumeM3, x.VolumetricWeightKg, x.DeclaredValue,
        x.IsFragile, x.IsHazardous, x.RequiresRefrigeration,
        x.StackingInstructions, x.CreatedAt, x.UpdatedAt
    );
}
