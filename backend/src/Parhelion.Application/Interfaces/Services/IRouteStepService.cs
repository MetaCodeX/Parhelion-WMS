using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Network;
using Parhelion.Domain.Entities;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de pasos de ruta.
/// </summary>
public interface IRouteStepService : IGenericService<RouteStep, RouteStepResponse, CreateRouteStepRequest, UpdateRouteStepRequest>
{
    /// <summary>
    /// Obtiene pasos de una ruta específica ordenados.
    /// </summary>
    Task<IEnumerable<RouteStepResponse>> GetByRouteAsync(Guid routeBlueprintId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reordena los pasos de una ruta.
    /// </summary>
    Task<OperationResult> ReorderStepsAsync(Guid routeBlueprintId, IEnumerable<Guid> stepIdsInOrder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade un paso a una ruta.
    /// </summary>
    Task<OperationResult<RouteStepResponse>> AddStepToRouteAsync(Guid routeBlueprintId, CreateRouteStepRequest request, CancellationToken cancellationToken = default);
}
