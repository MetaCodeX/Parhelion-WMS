using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Domain.Entities;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de FleetLogs (bitácora de asignaciones).
/// </summary>
public interface IFleetLogService
{
    /// <summary>
    /// Obtiene logs con paginación.
    /// </summary>
    Task<PagedResult<FleetLogResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un log por ID.
    /// </summary>
    Task<FleetLogResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene historial de un chofer.
    /// </summary>
    Task<PagedResult<FleetLogResponse>> GetByDriverAsync(Guid driverId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene historial de un camión.
    /// </summary>
    Task<PagedResult<FleetLogResponse>> GetByTruckAsync(Guid truckId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra inicio de uso de un camión por un chofer.
    /// </summary>
    Task<OperationResult<FleetLogResponse>> StartUsageAsync(Guid driverId, Guid truckId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra fin de uso de un camión.
    /// </summary>
    Task<OperationResult<FleetLogResponse>> EndUsageAsync(Guid logId, decimal? endOdometer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el log activo de un chofer (sin EndAt).
    /// </summary>
    Task<FleetLogResponse?> GetActiveLogForDriverAsync(Guid driverId, CancellationToken cancellationToken = default);
}
