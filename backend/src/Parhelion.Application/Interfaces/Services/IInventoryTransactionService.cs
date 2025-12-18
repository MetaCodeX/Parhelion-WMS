using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para transacciones de inventario.
/// </summary>
public interface IInventoryTransactionService
{
    /// <summary>
    /// Obtiene transacciones por tenant.
    /// </summary>
    Task<PagedResult<InventoryTransactionResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene transacción por ID.
    /// </summary>
    Task<InventoryTransactionResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene transacciones por producto.
    /// </summary>
    Task<PagedResult<InventoryTransactionResponse>> GetByProductAsync(Guid productId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene transacciones por zona.
    /// </summary>
    Task<PagedResult<InventoryTransactionResponse>> GetByZoneAsync(Guid zoneId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene transacciones por tipo.
    /// </summary>
    Task<PagedResult<InventoryTransactionResponse>> GetByTypeAsync(Guid tenantId, InventoryTransactionType type, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra entrada de inventario.
    /// </summary>
    Task<OperationResult<InventoryTransactionResponse>> RecordReceiptAsync(CreateInventoryTransactionRequest request, Guid performedByUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra salida de inventario.
    /// </summary>
    Task<OperationResult<InventoryTransactionResponse>> RecordDispatchAsync(CreateInventoryTransactionRequest request, Guid performedByUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra transferencia entre zonas.
    /// </summary>
    Task<OperationResult<InventoryTransactionResponse>> RecordTransferAsync(CreateInventoryTransactionRequest request, Guid performedByUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe la transacción.
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
