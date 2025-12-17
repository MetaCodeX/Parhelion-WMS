using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Enums;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de clientes (remitentes/destinatarios).
/// </summary>
[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    /// <summary>
    /// Inicializa el controlador con el servicio de Clients.
    /// </summary>
    /// <param name="clientService">Servicio de gestión de clientes.</param>
    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    /// <summary>
    /// Obtiene todos los clientes con paginación.
    /// </summary>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista paginada de clientes.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ClientResponse>>> GetAll(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _clientService.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un cliente por ID.
    /// </summary>
    /// <param name="id">ID del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Cliente encontrado.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClientResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _clientService.GetByIdAsync(id, cancellationToken);
        if (item == null)
            return NotFound(new { error = "Cliente no encontrado" });

        return Ok(item);
    }

    /// <summary>
    /// Busca un cliente por email.
    /// </summary>
    /// <param name="email">Email del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Cliente encontrado.</returns>
    [HttpGet("by-email")]
    public async Task<ActionResult<ClientResponse>> GetByEmail(
        [FromQuery] string email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { error = "El parámetro 'email' es requerido" });

        var item = await _clientService.GetByEmailAsync(email, cancellationToken);
        if (item == null)
            return NotFound(new { error = "Cliente no encontrado" });

        return Ok(item);
    }

    /// <summary>
    /// Busca un cliente por Tax ID (RFC).
    /// </summary>
    /// <param name="taxId">RFC del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Cliente encontrado.</returns>
    [HttpGet("by-tax-id/{taxId}")]
    public async Task<ActionResult<ClientResponse>> GetByTaxId(
        string taxId,
        CancellationToken cancellationToken = default)
    {
        var item = await _clientService.GetByTaxIdAsync(taxId, cancellationToken);
        if (item == null)
            return NotFound(new { error = "Cliente no encontrado" });

        return Ok(item);
    }

    /// <summary>
    /// Obtiene clientes del tenant actual.
    /// </summary>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista paginada de clientes del tenant.</returns>
    [HttpGet("current-tenant")]
    public async Task<ActionResult<PagedResult<ClientResponse>>> GetByCurrentTenant(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var result = await _clientService.GetByTenantAsync(tenantId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene clientes por prioridad del tenant actual.
    /// </summary>
    /// <param name="priority">Prioridad del cliente (Normal, Low, High, Urgent).</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista paginada de clientes con la prioridad especificada.</returns>
    [HttpGet("by-priority/{priority}")]
    public async Task<ActionResult<PagedResult<ClientResponse>>> GetByPriority(
        string priority,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ClientPriority>(priority, out var clientPriority))
            return BadRequest(new { error = "Prioridad inválida" });

        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var result = await _clientService.GetByPriorityAsync(
            tenantId, clientPriority, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Busca clientes por nombre de empresa.
    /// </summary>
    /// <param name="companyName">Nombre parcial de la empresa.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista paginada de clientes que coinciden.</returns>
    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<ClientResponse>>> Search(
        [FromQuery] string companyName,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            return BadRequest(new { error = "El parámetro 'companyName' es requerido" });

        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var result = await _clientService.SearchByCompanyNameAsync(
            tenantId, companyName, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo cliente.
    /// </summary>
    /// <param name="request">Datos del nuevo cliente.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Cliente creado.</returns>
    [HttpPost]
    public async Task<ActionResult<ClientResponse>> Create(
        [FromBody] CreateClientRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _clientService.CreateAsync(request, cancellationToken);
        
        if (!result.Success)
            return Conflict(new { error = result.Message });

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            result.Data);
    }

    /// <summary>
    /// Actualiza un cliente existente.
    /// </summary>
    /// <param name="id">ID del cliente.</param>
    /// <param name="request">Datos de actualización.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Cliente actualizado.</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClientResponse>> Update(
        Guid id,
        [FromBody] UpdateClientRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _clientService.UpdateAsync(id, request, cancellationToken);
        
        if (!result.Success)
        {
            if (result.Message?.Contains("no encontrado") == true)
                return NotFound(new { error = result.Message });
            return Conflict(new { error = result.Message });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Elimina (soft-delete) un cliente.
    /// </summary>
    /// <param name="id">ID del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _clientService.DeleteAsync(id, cancellationToken);
        
        if (!result.Success)
            return NotFound(new { error = result.Message });

        return NoContent();
    }
}
