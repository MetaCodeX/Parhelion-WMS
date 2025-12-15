using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para transacciones de inventario (Kardex).
/// Las transacciones son inmutables - solo se crean y consultan.
/// </summary>
[ApiController]
[Route("api/inventory-transactions")]
[Authorize]
public class InventoryTransactionsController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public InventoryTransactionsController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryTransactionResponse>>> GetAll()
    {
        var items = await _context.InventoryTransactions
            .Include(x => x.Product)
            .Include(x => x.OriginZone)
            .Include(x => x.DestinationZone)
            .Include(x => x.PerformedBy)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.Timestamp)
            .Take(100)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InventoryTransactionResponse>> GetById(Guid id)
    {
        var item = await _context.InventoryTransactions
            .Include(x => x.Product)
            .Include(x => x.OriginZone)
            .Include(x => x.DestinationZone)
            .Include(x => x.PerformedBy)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Transacción no encontrada" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("by-product/{productId:guid}")]
    public async Task<ActionResult<IEnumerable<InventoryTransactionResponse>>> ByProduct(Guid productId)
    {
        var items = await _context.InventoryTransactions
            .Include(x => x.Product)
            .Include(x => x.OriginZone)
            .Include(x => x.DestinationZone)
            .Include(x => x.PerformedBy)
            .Where(x => !x.IsDeleted && x.ProductId == productId)
            .OrderByDescending(x => x.Timestamp)
            .Take(100)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("by-zone/{zoneId:guid}")]
    public async Task<ActionResult<IEnumerable<InventoryTransactionResponse>>> ByZone(Guid zoneId)
    {
        var items = await _context.InventoryTransactions
            .Include(x => x.Product)
            .Include(x => x.OriginZone)
            .Include(x => x.DestinationZone)
            .Include(x => x.PerformedBy)
            .Where(x => !x.IsDeleted && (x.OriginZoneId == zoneId || x.DestinationZoneId == zoneId))
            .OrderByDescending(x => x.Timestamp)
            .Take(100)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<InventoryTransactionResponse>> Create([FromBody] CreateInventoryTransactionRequest request)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new { error = "No se pudo determinar el usuario" });

        var item = new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductId = request.ProductId,
            OriginZoneId = request.OriginZoneId,
            DestinationZoneId = request.DestinationZoneId,
            Quantity = request.Quantity,
            TransactionType = Enum.TryParse<InventoryTransactionType>(request.TransactionType, out var t) 
                ? t : InventoryTransactionType.Adjustment,
            PerformedByUserId = userId,
            ShipmentId = request.ShipmentId,
            BatchNumber = request.BatchNumber,
            Remarks = request.Remarks,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.InventoryTransactions.Add(item);
        await _context.SaveChangesAsync();

        item = await _context.InventoryTransactions
            .Include(x => x.Product)
            .Include(x => x.OriginZone)
            .Include(x => x.DestinationZone)
            .Include(x => x.PerformedBy)
            .FirstAsync(x => x.Id == item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    // No PUT/DELETE - transactions are immutable

    private static InventoryTransactionResponse MapToResponse(InventoryTransaction x) => new(
        x.Id, x.ProductId, x.Product?.Name ?? "",
        x.OriginZoneId, x.OriginZone?.Name,
        x.DestinationZoneId, x.DestinationZone?.Name,
        x.Quantity, x.TransactionType.ToString(),
        x.PerformedByUserId, x.PerformedBy?.FullName ?? "",
        x.ShipmentId, x.BatchNumber, x.Remarks,
        x.Timestamp, x.CreatedAt
    );
}
