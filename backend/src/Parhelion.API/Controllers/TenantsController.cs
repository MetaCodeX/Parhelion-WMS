using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Core;
using Parhelion.Domain.Entities;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de Tenants (empresas clientes del sistema).
/// Solo accesible por Super Admins.
/// </summary>
[ApiController]
[Route("api/tenants")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public TenantsController(ParhelionDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todos los tenants (solo Super Admin).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantResponse>>> GetAll()
    {
        var items = await _context.Tenants
            .IgnoreQueryFilters()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.CompanyName)
            .Select(x => MapToResponse(x))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Obtiene el tenant actual del usuario logueado.
    /// </summary>
    [HttpGet("current")]
    public async Task<ActionResult<TenantResponse>> GetCurrent()
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            return Unauthorized(new { error = "No se pudo determinar el tenant" });
        }

        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == tenantId && !x.IsDeleted);

        if (tenant == null)
            return NotFound(new { error = "Tenant no encontrado" });

        return Ok(MapToResponse(tenant));
    }

    /// <summary>
    /// Obtiene un tenant por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TenantResponse>> GetById(Guid id)
    {
        var item = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
            return NotFound(new { error = "Tenant no encontrado" });

        return Ok(MapToResponse(item));
    }

    /// <summary>
    /// Crea un nuevo tenant.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TenantResponse>> Create([FromBody] CreateTenantRequest request)
    {
        var existing = await _context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(x => x.ContactEmail == request.ContactEmail && !x.IsDeleted);

        if (existing)
            return Conflict(new { error = "Ya existe un tenant con ese email" });

        var item = new Tenant
        {
            Id = Guid.NewGuid(),
            CompanyName = request.CompanyName,
            ContactEmail = request.ContactEmail,
            FleetSize = request.FleetSize,
            DriverCount = request.DriverCount,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tenants.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    /// <summary>
    /// Actualiza un tenant existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TenantResponse>> Update(Guid id, [FromBody] UpdateTenantRequest request)
    {
        var item = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
            return NotFound(new { error = "Tenant no encontrado" });

        item.CompanyName = request.CompanyName;
        item.ContactEmail = request.ContactEmail;
        item.FleetSize = request.FleetSize;
        item.DriverCount = request.DriverCount;
        item.IsActive = request.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(item));
    }

    /// <summary>
    /// Elimina (soft-delete) un tenant.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
            return NotFound(new { error = "Tenant no encontrado" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static TenantResponse MapToResponse(Tenant x) => new(
        x.Id, x.CompanyName, x.ContactEmail, x.FleetSize, x.DriverCount,
        x.IsActive, x.CreatedAt, x.UpdatedAt
    );
}
