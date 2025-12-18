using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Domain.Entities;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de inventario físico.
/// </summary>
public interface IInventoryStockService : IGenericService<InventoryStock, InventoryStockResponse, CreateInventoryStockRequest, UpdateInventoryStockRequest>
{
    /// <summary>
    /// Obtiene stock por tenant.
    /// </summary>
    Task<PagedResult<InventoryStockResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene stock por zona.
    /// </summary>
    Task<PagedResult<InventoryStockResponse>> GetByZoneAsync(Guid zoneId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene stock por producto.
    /// </summary>
    Task<PagedResult<InventoryStockResponse>> GetByProductAsync(Guid productId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene productos con bajo stock.
    /// </summary>
    Task<PagedResult<InventoryStockResponse>> GetLowStockAsync(Guid tenantId, decimal threshold, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reserva cantidad de un producto.
    /// </summary>
    Task<OperationResult<InventoryStockResponse>> ReserveQuantityAsync(Guid stockId, decimal quantity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Libera cantidad reservada.
    /// </summary>
    Task<OperationResult<InventoryStockResponse>> ReleaseReservedAsync(Guid stockId, decimal quantity, CancellationToken cancellationToken = default);
}
