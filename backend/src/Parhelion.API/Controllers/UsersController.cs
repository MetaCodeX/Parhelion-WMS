using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Application.Interfaces.Services;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de usuarios.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    /// <summary>
    /// Inicializa el controlador con el servicio de Users.
    /// </summary>
    /// <param name="userService">Servicio de gestión de usuarios.</param>
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Obtiene todos los usuarios con paginación.
    /// </summary>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista paginada de usuarios.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserResponse>>> GetAll(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un usuario por ID.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Usuario encontrado.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _userService.GetByIdAsync(id, cancellationToken);
        if (item == null)
            return NotFound(new { error = "Usuario no encontrado" });

        return Ok(item);
    }

    /// <summary>
    /// Obtiene usuarios del tenant actual.
    /// </summary>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista paginada de usuarios del tenant.</returns>
    [HttpGet("current-tenant")]
    public async Task<ActionResult<PagedResult<UserResponse>>> GetByCurrentTenant(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var result = await _userService.GetByTenantAsync(tenantId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Busca un usuario por email.
    /// </summary>
    /// <param name="email">Email del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Usuario encontrado.</returns>
    [HttpGet("by-email")]
    public async Task<ActionResult<UserResponse>> GetByEmail(
        [FromQuery] string email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { error = "El parámetro 'email' es requerido" });

        var item = await _userService.GetByEmailAsync(email, cancellationToken);
        if (item == null)
            return NotFound(new { error = "Usuario no encontrado" });

        return Ok(item);
    }

    /// <summary>
    /// Crea un nuevo usuario.
    /// </summary>
    /// <param name="request">Datos del nuevo usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Usuario creado.</returns>
    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.CreateAsync(request, cancellationToken);
        
        if (!result.Success)
            return Conflict(new { error = result.Message });

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            result.Data);
    }

    /// <summary>
    /// Actualiza un usuario existente.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="request">Datos de actualización.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Usuario actualizado.</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserResponse>> Update(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.UpdateAsync(id, request, cancellationToken);
        
        if (!result.Success)
        {
            if (result.Message?.Contains("no encontrado") == true)
                return NotFound(new { error = result.Message });
            return Conflict(new { error = result.Message });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Cambia el password del usuario actual.
    /// </summary>
    /// <param name="currentPassword">Password actual.</param>
    /// <param name="newPassword">Nuevo password.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    [HttpPatch("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromQuery] string currentPassword,
        [FromQuery] string newPassword,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("user_id");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new { error = "No se pudo determinar el usuario" });

        var result = await _userService.ChangePasswordAsync(
            userId, currentPassword, newPassword, cancellationToken);
        
        if (!result.Success)
            return BadRequest(new { error = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Elimina (soft-delete) un usuario.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.DeleteAsync(id, cancellationToken);
        
        if (!result.Success)
            return NotFound(new { error = result.Message });

        return NoContent();
    }
}
