using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Catalog;
using Parhelion.Domain.Entities;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador CRUD para el Catálogo de Productos.
/// Gestiona los SKUs y sus propiedades (dimensiones, manejo especial, etc).
/// </summary>
[ApiController]
[Route("api/catalog-items")]
[Authorize]
public class CatalogItemsController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public CatalogItemsController(ParhelionDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todos los CatalogItems del tenant.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CatalogItemResponse>>> GetAll()
    {
        var items = await _context.CatalogItems
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Sku)
            .Select(x => MapToResponse(x))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Obtiene un CatalogItem por su ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CatalogItemResponse>> GetById(Guid id)
    {
        var item = await _context.CatalogItems
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
        {
            return NotFound(new { error = "CatalogItem no encontrado" });
        }

        return Ok(MapToResponse(item));
    }

    /// <summary>
    /// Busca CatalogItems por SKU (búsqueda parcial).
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<CatalogItemResponse>>> SearchBySku([FromQuery] string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return BadRequest(new { error = "El parámetro 'sku' es requerido" });
        }

        var items = await _context.CatalogItems
            .Where(x => !x.IsDeleted && x.Sku.ToLower().Contains(sku.ToLower()))
            .OrderBy(x => x.Sku)
            .Select(x => MapToResponse(x))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Crea un nuevo CatalogItem.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CatalogItemResponse>> Create([FromBody] CreateCatalogItemRequest request)
    {
        // Verificar que el SKU no exista ya en el tenant
        var existingSku = await _context.CatalogItems
            .AnyAsync(x => x.Sku == request.Sku && !x.IsDeleted);

        if (existingSku)
        {
            return Conflict(new { error = $"Ya existe un producto con SKU '{request.Sku}'" });
        }

        // Obtener TenantId del usuario actual
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            return Unauthorized(new { error = "No se pudo determinar el tenant del usuario" });
        }

        var item = new CatalogItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            BaseUom = request.BaseUom,
            DefaultWeightKg = request.DefaultWeightKg,
            DefaultWidthCm = request.DefaultWidthCm,
            DefaultHeightCm = request.DefaultHeightCm,
            DefaultLengthCm = request.DefaultLengthCm,
            RequiresRefrigeration = request.RequiresRefrigeration,
            IsHazardous = request.IsHazardous,
            IsFragile = request.IsFragile,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.CatalogItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.Id },
            MapToResponse(item));
    }

    /// <summary>
    /// Actualiza un CatalogItem existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CatalogItemResponse>> Update(Guid id, [FromBody] UpdateCatalogItemRequest request)
    {
        var item = await _context.CatalogItems
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
        {
            return NotFound(new { error = "CatalogItem no encontrado" });
        }

        item.Name = request.Name;
        item.Description = request.Description;
        item.BaseUom = request.BaseUom;
        item.DefaultWeightKg = request.DefaultWeightKg;
        item.DefaultWidthCm = request.DefaultWidthCm;
        item.DefaultHeightCm = request.DefaultHeightCm;
        item.DefaultLengthCm = request.DefaultLengthCm;
        item.RequiresRefrigeration = request.RequiresRefrigeration;
        item.IsHazardous = request.IsHazardous;
        item.IsFragile = request.IsFragile;
        item.IsActive = request.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapToResponse(item));
    }

    /// <summary>
    /// Elimina (soft-delete) un CatalogItem.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.CatalogItems
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
        {
            return NotFound(new { error = "CatalogItem no encontrado" });
        }

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ========== Mapping Helper ==========

    private static CatalogItemResponse MapToResponse(CatalogItem item) => new(
        item.Id,
        item.Sku,
        item.Name,
        item.Description,
        item.BaseUom,
        item.DefaultWeightKg,
        item.DefaultWidthCm,
        item.DefaultHeightCm,
        item.DefaultLengthCm,
        item.DefaultVolumeM3,
        item.RequiresRefrigeration,
        item.IsHazardous,
        item.IsFragile,
        item.IsActive,
        item.CreatedAt,
        item.UpdatedAt
    );
}
