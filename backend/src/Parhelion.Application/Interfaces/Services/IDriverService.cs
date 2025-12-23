using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de Drivers (choferes).
/// </summary>
public interface IDriverService : IGenericService<Driver, DriverResponse, CreateDriverRequest, UpdateDriverRequest>
{
    /// <summary>
    /// Obtiene un chofer por su Employee ID.
    /// </summary>
    Task<DriverResponse?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene choferes por tenant.
    /// </summary>
    Task<PagedResult<DriverResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene choferes por estatus.
    /// </summary>
    Task<PagedResult<DriverResponse>> GetByStatusAsync(Guid tenantId, DriverStatus status, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene choferes disponibles.
    /// </summary>
    Task<PagedResult<DriverResponse>> GetAvailableAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza el estatus de un chofer.
    /// </summary>
    Task<OperationResult<DriverResponse>> UpdateStatusAsync(Guid id, DriverStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asigna un camión al chofer.
    /// </summary>
    Task<OperationResult<DriverResponse>> AssignTruckAsync(Guid driverId, Guid truckId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Busca choferes cercanos a una coordenada (Crisis Management).
    /// </summary>
    Task<PagedResult<DriverResponse>> GetNearbyDriversAsync(decimal lat, decimal lon, double radiusKm, Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);
}
