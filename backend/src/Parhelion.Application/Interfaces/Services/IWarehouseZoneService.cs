using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Domain.Entities;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de zonas de almacén.
/// </summary>
public interface IWarehouseZoneService : IGenericService<WarehouseZone, WarehouseZoneResponse, CreateWarehouseZoneRequest, UpdateWarehouseZoneRequest>
{
    /// <summary>
    /// Obtiene zonas por ubicación/bodega.
    /// </summary>
    Task<PagedResult<WarehouseZoneResponse>> GetByLocationAsync(Guid locationId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene zonas activas de una ubicación.
    /// </summary>
    Task<PagedResult<WarehouseZoneResponse>> GetActiveAsync(Guid locationId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene zona por código.
    /// </summary>
    Task<WarehouseZoneResponse?> GetByCodeAsync(Guid locationId, string code, CancellationToken cancellationToken = default);
}
