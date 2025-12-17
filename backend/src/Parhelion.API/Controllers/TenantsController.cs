using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Application.Interfaces.Services;

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
    private readonly ITenantService _tenantService;

    /// <summary>
    /// Inicializa el controlador con el servicio de Tenants.
    /// </summary>
    /// <param name="tenantService">Servicio de gestión de tenants.</param>
    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    /// <summary>
    /// Obtiene todos los tenants con paginación.
    /// </summary>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista paginada de tenants.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<TenantResponse>>> GetAll(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _tenantService.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el tenant actual del usuario logueado.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tenant actual.</returns>
    [HttpGet("current")]
    public async Task<ActionResult<TenantResponse>> GetCurrent(
        CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            return Unauthorized(new { error = "No se pudo determinar el tenant" });
        }

        var tenant = await _tenantService.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null)
            return NotFound(new { error = "Tenant no encontrado" });

        return Ok(tenant);
    }

    /// <summary>
    /// Obtiene un tenant por ID.
    /// </summary>
    /// <param name="id">ID del tenant.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tenant encontrado.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TenantResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _tenantService.GetByIdAsync(id, cancellationToken);
        if (item == null)
            return NotFound(new { error = "Tenant no encontrado" });

        return Ok(item);
    }

    /// <summary>
    /// Obtiene solo los tenants activos.
    /// </summary>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista paginada de tenants activos.</returns>
    [HttpGet("active")]
    public async Task<ActionResult<PagedResult<TenantResponse>>> GetActive(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _tenantService.GetActiveAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo tenant.
    /// </summary>
    /// <param name="request">Datos del nuevo tenant.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tenant creado.</returns>
    [HttpPost]
    public async Task<ActionResult<TenantResponse>> Create(
        [FromBody] CreateTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _tenantService.CreateAsync(request, cancellationToken);
        
        if (!result.Success)
            return Conflict(new { error = result.Message });

        return CreatedAtAction(
            nameof(GetById), 
            new { id = result.Data!.Id }, 
            result.Data);
    }

    /// <summary>
    /// Actualiza un tenant existente.
    /// </summary>
    /// <param name="id">ID del tenant.</param>
    /// <param name="request">Datos de actualización.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tenant actualizado.</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TenantResponse>> Update(
        Guid id,
        [FromBody] UpdateTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _tenantService.UpdateAsync(id, request, cancellationToken);
        
        if (!result.Success)
        {
            if (result.Message?.Contains("no encontrado") == true)
                return NotFound(new { error = result.Message });
            return Conflict(new { error = result.Message });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Activa o desactiva un tenant.
    /// </summary>
    /// <param name="id">ID del tenant.</param>
    /// <param name="isActive">Estado deseado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(
        Guid id,
        [FromQuery] bool isActive,
        CancellationToken cancellationToken = default)
    {
        var result = await _tenantService.SetActiveStatusAsync(id, isActive, cancellationToken);
        
        if (!result.Success)
            return NotFound(new { error = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Elimina (soft-delete) un tenant.
    /// </summary>
    /// <param name="id">ID del tenant.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _tenantService.DeleteAsync(id, cancellationToken);
        
        if (!result.Success)
            return NotFound(new { error = result.Message });

        return NoContent();
    }
}
