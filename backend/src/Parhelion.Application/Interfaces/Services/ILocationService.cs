using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de Locations (ubicaciones/nodos de la red).
/// </summary>
public interface ILocationService : IGenericService<Location, LocationResponse, CreateLocationRequest, UpdateLocationRequest>
{
    /// <summary>
    /// Obtiene ubicaciones por tenant.
    /// </summary>
    Task<PagedResult<LocationResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene ubicaciones por tipo.
    /// </summary>
    Task<PagedResult<LocationResponse>> GetByTypeAsync(Guid tenantId, LocationType locationType, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene ubicaciones activas.
    /// </summary>
    Task<PagedResult<LocationResponse>> GetActiveAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca ubicaciones por nombre.
    /// </summary>
    Task<PagedResult<LocationResponse>> SearchByNameAsync(Guid tenantId, string name, PagedRequest request, CancellationToken cancellationToken = default);
}
