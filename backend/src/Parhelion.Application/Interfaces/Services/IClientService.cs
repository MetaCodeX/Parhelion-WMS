using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de Clients (remitentes/destinatarios de envíos).
/// Los Clients son empresas externas con las que se interactúa comercialmente.
/// </summary>
public interface IClientService : IGenericService<Client, ClientResponse, CreateClientRequest, UpdateClientRequest>
{
    /// <summary>
    /// Busca un cliente por su email.
    /// </summary>
    /// <param name="email">Email del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Cliente encontrado o null.</returns>
    Task<ClientResponse?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca un cliente por su Tax ID (RFC).
    /// </summary>
    /// <param name="taxId">RFC del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Cliente encontrado o null.</returns>
    Task<ClientResponse?> GetByTaxIdAsync(
        string taxId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene clientes por tenant.
    /// </summary>
    /// <param name="tenantId">ID del tenant.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de clientes del tenant.</returns>
    Task<PagedResult<ClientResponse>> GetByTenantAsync(
        Guid tenantId,
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene clientes por prioridad.
    /// </summary>
    /// <param name="tenantId">ID del tenant.</param>
    /// <param name="priority">Prioridad del cliente.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de clientes con la prioridad especificada.</returns>
    Task<PagedResult<ClientResponse>> GetByPriorityAsync(
        Guid tenantId,
        ClientPriority priority,
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca clientes por nombre de empresa (búsqueda parcial).
    /// </summary>
    /// <param name="tenantId">ID del tenant.</param>
    /// <param name="companyName">Nombre parcial de la empresa.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de clientes que coinciden.</returns>
    Task<PagedResult<ClientResponse>> SearchByCompanyNameAsync(
        Guid tenantId,
        string companyName,
        PagedRequest request,
        CancellationToken cancellationToken = default);
}
