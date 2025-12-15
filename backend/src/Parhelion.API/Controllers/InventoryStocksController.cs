using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Domain.Entities;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para inventario (stocks).
/// </summary>
[ApiController]
[Route("api/inventory-stocks")]
[Authorize]
public class InventoryStocksController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public InventoryStocksController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryStockResponse>>> GetAll()
    {
        var items = await _context.InventoryStocks
            .Include(x => x.Zone)
            .Include(x => x.Product)
            .Where(x => !x.IsDeleted)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InventoryStockResponse>> GetById(Guid id)
    {
        var item = await _context.InventoryStocks
            .Include(x => x.Zone)
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Stock no encontrado" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("by-product/{productId:guid}")]
    public async Task<ActionResult<IEnumerable<InventoryStockResponse>>> ByProduct(Guid productId)
    {
        var items = await _context.InventoryStocks
            .Include(x => x.Zone)
            .Include(x => x.Product)
            .Where(x => !x.IsDeleted && x.ProductId == productId)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("by-zone/{zoneId:guid}")]
    public async Task<ActionResult<IEnumerable<InventoryStockResponse>>> ByZone(Guid zoneId)
    {
        var items = await _context.InventoryStocks
            .Include(x => x.Zone)
            .Include(x => x.Product)
            .Where(x => !x.IsDeleted && x.ZoneId == zoneId)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<InventoryStockResponse>> Create([FromBody] CreateInventoryStockRequest request)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var item = new InventoryStock
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ZoneId = request.ZoneId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            QuantityReserved = request.QuantityReserved,
            BatchNumber = request.BatchNumber,
            ExpiryDate = request.ExpiryDate,
            UnitCost = request.UnitCost,
            CreatedAt = DateTime.UtcNow
        };

        _context.InventoryStocks.Add(item);
        await _context.SaveChangesAsync();

        item = await _context.InventoryStocks
            .Include(x => x.Zone)
            .Include(x => x.Product)
            .FirstAsync(x => x.Id == item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InventoryStockResponse>> Update(Guid id, [FromBody] UpdateInventoryStockRequest request)
    {
        var item = await _context.InventoryStocks
            .Include(x => x.Zone)
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Stock no encontrado" });

        item.Quantity = request.Quantity;
        item.QuantityReserved = request.QuantityReserved;
        item.BatchNumber = request.BatchNumber;
        item.ExpiryDate = request.ExpiryDate;
        item.LastCountDate = request.LastCountDate;
        item.UnitCost = request.UnitCost;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.InventoryStocks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Stock no encontrado" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static InventoryStockResponse MapToResponse(InventoryStock x) => new(
        x.Id, x.ZoneId, x.Zone?.Name ?? "", x.ProductId, x.Product?.Name ?? "", x.Product?.Sku ?? "",
        x.Quantity, x.QuantityReserved, x.QuantityAvailable, x.BatchNumber, x.ExpiryDate,
        x.LastCountDate, x.UnitCost, x.CreatedAt, x.UpdatedAt
    );
}
