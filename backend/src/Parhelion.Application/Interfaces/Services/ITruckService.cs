using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de Trucks (camiones/unidades).
/// </summary>
public interface ITruckService : IGenericService<Truck, TruckResponse, CreateTruckRequest, UpdateTruckRequest>
{
    /// <summary>
    /// Busca un camión por placa.
    /// </summary>
    Task<TruckResponse?> GetByPlateAsync(string plate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene camiones por tenant.
    /// </summary>
    Task<PagedResult<TruckResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene camiones activos o inactivos.
    /// </summary>
    Task<PagedResult<TruckResponse>> GetByActiveStatusAsync(Guid tenantId, bool isActive, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene camiones por tipo.
    /// </summary>
    Task<PagedResult<TruckResponse>> GetByTypeAsync(Guid tenantId, TruckType truckType, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza el estatus activo de un camión.
    /// </summary>
    Task<OperationResult<TruckResponse>> SetActiveStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene camiones disponibles (activos).
    /// </summary>
    Task<PagedResult<TruckResponse>> GetAvailableAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);
}
