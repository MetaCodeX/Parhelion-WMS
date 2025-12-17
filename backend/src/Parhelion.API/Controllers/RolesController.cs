using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Enums;

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
    private readonly IRoleService _roleService;

    /// <summary>
    /// Inicializa el controlador con el servicio de Roles.
    /// </summary>
    /// <param name="roleService">Servicio de gestión de roles.</param>
    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// Obtiene todos los roles con paginación.
    /// </summary>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista paginada de roles.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<RoleResponse>>> GetAll(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _roleService.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un rol por ID.
    /// </summary>
    /// <param name="id">ID del rol.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Rol encontrado.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoleResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _roleService.GetByIdAsync(id, cancellationToken);
        if (item == null)
            return NotFound(new { error = "Rol no encontrado" });

        return Ok(item);
    }

    /// <summary>
    /// Busca un rol por nombre.
    /// </summary>
    /// <param name="name">Nombre del rol.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Rol encontrado.</returns>
    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<RoleResponse>> GetByName(
        string name,
        CancellationToken cancellationToken = default)
    {
        var item = await _roleService.GetByNameAsync(name, cancellationToken);
        if (item == null)
            return NotFound(new { error = "Rol no encontrado" });

        return Ok(item);
    }

    /// <summary>
    /// Obtiene los permisos de un rol.
    /// </summary>
    /// <param name="name">Nombre del rol.</param>
    /// <returns>Lista de permisos del rol.</returns>
    [HttpGet("{name}/permissions")]
    public ActionResult<IEnumerable<string>> GetPermissions(string name)
    {
        var permissions = _roleService.GetPermissions(name);
        return Ok(permissions.Select(p => p.ToString()));
    }

    /// <summary>
    /// Verifica si un rol tiene un permiso específico.
    /// </summary>
    /// <param name="name">Nombre del rol.</param>
    /// <param name="permission">Permiso a verificar.</param>
    /// <returns>True si el rol tiene el permiso.</returns>
    [HttpGet("{name}/has-permission/{permission}")]
    public ActionResult<bool> HasPermission(string name, string permission)
    {
        if (!Enum.TryParse<Permission>(permission, out var perm))
            return BadRequest(new { error = "Permiso inválido" });

        var hasPermission = _roleService.HasPermission(name, perm);
        return Ok(new { hasPermission });
    }

    /// <summary>
    /// Crea un nuevo rol.
    /// </summary>
    /// <param name="request">Datos del nuevo rol.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Rol creado.</returns>
    [HttpPost]
    public async Task<ActionResult<RoleResponse>> Create(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _roleService.CreateAsync(request, cancellationToken);
        
        if (!result.Success)
            return Conflict(new { error = result.Message });

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            result.Data);
    }

    /// <summary>
    /// Actualiza un rol existente.
    /// </summary>
    /// <param name="id">ID del rol.</param>
    /// <param name="request">Datos de actualización.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Rol actualizado.</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RoleResponse>> Update(
        Guid id,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _roleService.UpdateAsync(id, request, cancellationToken);
        
        if (!result.Success)
        {
            if (result.Message?.Contains("no encontrado") == true)
                return NotFound(new { error = result.Message });
            return Conflict(new { error = result.Message });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Elimina (soft-delete) un rol.
    /// </summary>
    /// <param name="id">ID del rol.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _roleService.DeleteAsync(id, cancellationToken);
        
        if (!result.Success)
        {
            if (result.Message?.Contains("usuarios asignados") == true)
                return Conflict(new { error = result.Message });
            return NotFound(new { error = result.Message });
        }

        return NoContent();
    }
}
