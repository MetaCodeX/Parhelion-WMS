using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Domain.Entities;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de Tenants (empresas clientes).
/// Los Tenants son entidades raíz del sistema multi-tenant.
/// </summary>
public interface ITenantService : IGenericService<Tenant, TenantResponse, CreateTenantRequest, UpdateTenantRequest>
{
    /// <summary>
    /// Busca un tenant por su email de contacto.
    /// </summary>
    /// <param name="email">Email de contacto del tenant.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tenant encontrado o null.</returns>
    Task<TenantResponse?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene solo los tenants activos.
    /// </summary>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de tenants activos.</returns>
    Task<PagedResult<TenantResponse>> GetActiveAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activa o desactiva un tenant.
    /// </summary>
    /// <param name="id">ID del tenant.</param>
    /// <param name="isActive">Estado deseado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<OperationResult> SetActiveStatusAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default);
}
