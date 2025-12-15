using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Core;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de clientes (remitentes/destinatarios).
/// </summary>
[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public ClientsController(ParhelionDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todos los clientes del tenant.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientResponse>>> GetAll()
    {
        var items = await _context.Clients
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.CompanyName)
            .Select(x => MapToResponse(x))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Obtiene un cliente por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClientResponse>> GetById(Guid id)
    {
        var item = await _context.Clients
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
            return NotFound(new { error = "Cliente no encontrado" });

        return Ok(MapToResponse(item));
    }

    /// <summary>
    /// Busca clientes por nombre o email.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ClientResponse>>> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { error = "El parámetro 'q' es requerido" });

        var query = q.ToLower();
        var items = await _context.Clients
            .Where(x => !x.IsDeleted && 
                (x.CompanyName.ToLower().Contains(query) || 
                 x.ContactName.ToLower().Contains(query) ||
                 x.Email.ToLower().Contains(query)))
            .OrderBy(x => x.CompanyName)
            .Select(x => MapToResponse(x))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Crea un nuevo cliente.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ClientResponse>> Create([FromBody] CreateClientRequest request)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var item = new Client
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyName = request.CompanyName,
            TradeName = request.TradeName,
            ContactName = request.ContactName,
            Email = request.Email,
            Phone = request.Phone,
            TaxId = request.TaxId,
            LegalName = request.LegalName,
            BillingAddress = request.BillingAddress,
            ShippingAddress = request.ShippingAddress,
            PreferredProductTypes = request.PreferredProductTypes,
            Priority = Enum.TryParse<ClientPriority>(request.Priority, out var priority) 
                ? priority : ClientPriority.Normal,
            Notes = request.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    /// <summary>
    /// Actualiza un cliente existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClientResponse>> Update(Guid id, [FromBody] UpdateClientRequest request)
    {
        var item = await _context.Clients
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
            return NotFound(new { error = "Cliente no encontrado" });

        item.CompanyName = request.CompanyName;
        item.TradeName = request.TradeName;
        item.ContactName = request.ContactName;
        item.Email = request.Email;
        item.Phone = request.Phone;
        item.TaxId = request.TaxId;
        item.LegalName = request.LegalName;
        item.BillingAddress = request.BillingAddress;
        item.ShippingAddress = request.ShippingAddress;
        item.PreferredProductTypes = request.PreferredProductTypes;
        item.Priority = Enum.TryParse<ClientPriority>(request.Priority, out var priority) 
            ? priority : ClientPriority.Normal;
        item.IsActive = request.IsActive;
        item.Notes = request.Notes;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(item));
    }

    /// <summary>
    /// Elimina (soft-delete) un cliente.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.Clients
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
            return NotFound(new { error = "Cliente no encontrado" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static ClientResponse MapToResponse(Client x) => new(
        x.Id, x.CompanyName, x.TradeName, x.ContactName, x.Email, x.Phone,
        x.TaxId, x.LegalName, x.BillingAddress, x.ShippingAddress,
        x.PreferredProductTypes, x.Priority.ToString(), x.IsActive, x.Notes,
        x.CreatedAt, x.UpdatedAt
    );
}
