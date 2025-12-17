using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Network;
using Parhelion.Domain.Entities;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de Routes (rutas de transporte).
/// </summary>
public interface IRouteService : IGenericService<RouteBlueprint, RouteBlueprintResponse, CreateRouteBlueprintRequest, UpdateRouteBlueprintRequest>
{
    /// <summary>
    /// Obtiene rutas por tenant.
    /// </summary>
    Task<PagedResult<RouteBlueprintResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene rutas activas.
    /// </summary>
    Task<PagedResult<RouteBlueprintResponse>> GetActiveAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca rutas por nombre.
    /// </summary>
    Task<PagedResult<RouteBlueprintResponse>> SearchByNameAsync(Guid tenantId, string name, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los pasos de una ruta.
    /// </summary>
    Task<IEnumerable<RouteStepResponse>> GetStepsAsync(Guid routeId, CancellationToken cancellationToken = default);
}
