using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Core;
using Parhelion.Domain.Entities;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de roles.
/// Los roles son globales (no multi-tenant).
/// </summary>
[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public RolesController(ParhelionDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todos los roles.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleResponse>>> GetAll()
    {
        var items = await _context.Roles
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new RoleResponse(x.Id, x.Name, x.Description, x.CreatedAt))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Obtiene un rol por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoleResponse>> GetById(Guid id)
    {
        var item = await _context.Roles
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
            return NotFound(new { error = "Rol no encontrado" });

        return Ok(new RoleResponse(item.Id, item.Name, item.Description, item.CreatedAt));
    }

    /// <summary>
    /// Crea un nuevo rol.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RoleResponse>> Create([FromBody] CreateRoleRequest request)
    {
        var existing = await _context.Roles
            .AnyAsync(x => x.Name == request.Name && !x.IsDeleted);

        if (existing)
            return Conflict(new { error = "Ya existe un rol con ese nombre" });

        var item = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, 
            new RoleResponse(item.Id, item.Name, item.Description, item.CreatedAt));
    }

    /// <summary>
    /// Actualiza un rol existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RoleResponse>> Update(Guid id, [FromBody] UpdateRoleRequest request)
    {
        var item = await _context.Roles
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
            return NotFound(new { error = "Rol no encontrado" });

        item.Name = request.Name;
        item.Description = request.Description;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new RoleResponse(item.Id, item.Name, item.Description, item.CreatedAt));
    }

    /// <summary>
    /// Elimina (soft-delete) un rol.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.Roles
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
            return NotFound(new { error = "Rol no encontrado" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
