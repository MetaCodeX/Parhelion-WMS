using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.Auth;
using Parhelion.Application.DTOs.Core;
using Parhelion.Domain.Entities;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de usuarios.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ParhelionDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UsersController(ParhelionDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Obtiene todos los usuarios del tenant.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
    {
        var items = await _context.Users
            .Include(x => x.Role)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.FullName)
            .Select(x => MapToResponse(x))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Obtiene un usuario por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id)
    {
        var item = await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
            return NotFound(new { error = "Usuario no encontrado" });

        return Ok(MapToResponse(item));
    }

    /// <summary>
    /// Busca usuarios por nombre o email.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { error = "El parámetro 'q' es requerido" });

        var query = q.ToLower();
        var items = await _context.Users
            .Include(x => x.Role)
            .Where(x => !x.IsDeleted && 
                (x.FullName.ToLower().Contains(query) || x.Email.ToLower().Contains(query)))
            .OrderBy(x => x.FullName)
            .Select(x => MapToResponse(x))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Crea un nuevo usuario.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request)
    {
        var existingEmail = await _context.Users
            .AnyAsync(x => x.Email == request.Email && !x.IsDeleted);

        if (existingEmail)
            return Conflict(new { error = "Ya existe un usuario con ese email" });

        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var item = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FullName = request.FullName,
            RoleId = request.RoleId,
            IsDemoUser = request.IsDemoUser,
            UsesArgon2 = false,
            IsSuperAdmin = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(item);
        await _context.SaveChangesAsync();

        // Reload with Role
        item = await _context.Users.Include(x => x.Role).FirstAsync(x => x.Id == item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    /// <summary>
    /// Actualiza un usuario existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var item = await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
            return NotFound(new { error = "Usuario no encontrado" });

        item.FullName = request.FullName;
        item.RoleId = request.RoleId;
        item.IsActive = request.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload with updated Role
        item = await _context.Users.Include(x => x.Role).FirstAsync(x => x.Id == item.Id);
        return Ok(MapToResponse(item));
    }

    /// <summary>
    /// Elimina (soft-delete) un usuario.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
            return NotFound(new { error = "Usuario no encontrado" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static UserResponse MapToResponse(User x) => new(
        x.Id, x.Email, x.FullName, x.RoleId, x.Role?.Name ?? "",
        x.IsDemoUser, x.IsSuperAdmin, x.LastLogin, x.IsActive,
        x.CreatedAt, x.UpdatedAt
    );
}
