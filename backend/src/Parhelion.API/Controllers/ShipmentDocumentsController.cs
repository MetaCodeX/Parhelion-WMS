using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para documentos de envío.
/// </summary>
[ApiController]
[Route("api/shipment-documents")]
[Authorize]
public class ShipmentDocumentsController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public ShipmentDocumentsController(ParhelionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShipmentDocumentResponse>>> GetAll()
    {
        var items = await _context.ShipmentDocuments
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.GeneratedAt)
            .Take(100)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShipmentDocumentResponse>> GetById(Guid id)
    {
        var item = await _context.ShipmentDocuments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Documento no encontrado" });
        return Ok(MapToResponse(item));
    }

    [HttpGet("by-shipment/{shipmentId:guid}")]
    public async Task<ActionResult<IEnumerable<ShipmentDocumentResponse>>> ByShipment(Guid shipmentId)
    {
        var items = await _context.ShipmentDocuments
            .Where(x => !x.IsDeleted && x.ShipmentId == shipmentId)
            .OrderByDescending(x => x.GeneratedAt)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("by-type/{type}")]
    public async Task<ActionResult<IEnumerable<ShipmentDocumentResponse>>> ByType(string type)
    {
        if (!Enum.TryParse<DocumentType>(type, out var docType))
            return BadRequest(new { error = "Tipo de documento inválido" });

        var items = await _context.ShipmentDocuments
            .Where(x => !x.IsDeleted && x.DocumentType == docType)
            .OrderByDescending(x => x.GeneratedAt)
            .Take(100)
            .Select(x => MapToResponse(x))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<ShipmentDocumentResponse>> Create([FromBody] CreateShipmentDocumentRequest request)
    {
        var item = new ShipmentDocument
        {
            Id = Guid.NewGuid(),
            ShipmentId = request.ShipmentId,
            DocumentType = Enum.TryParse<DocumentType>(request.DocumentType, out var dt) ? dt : DocumentType.ServiceOrder,
            FileUrl = request.FileUrl,
            GeneratedBy = request.GeneratedBy,
            GeneratedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        _context.ShipmentDocuments.Add(item);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.ShipmentDocuments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (item == null) return NotFound(new { error = "Documento no encontrado" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static ShipmentDocumentResponse MapToResponse(ShipmentDocument x) => new(
        x.Id, x.ShipmentId, x.DocumentType.ToString(),
        x.FileUrl, x.GeneratedBy, x.GeneratedAt, x.ExpiresAt, x.CreatedAt
    );
}
