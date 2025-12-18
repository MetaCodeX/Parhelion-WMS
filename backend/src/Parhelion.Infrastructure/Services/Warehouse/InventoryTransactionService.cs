using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Services.Warehouse;

/// <summary>
/// Implementación del servicio de transacciones de inventario.
/// </summary>
public class InventoryTransactionService : IInventoryTransactionService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryTransactionService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<InventoryTransactionResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.InventoryTransactions.GetPagedAsync(request, filter: t => t.TenantId == tenantId, orderBy: q => q.OrderByDescending(t => t.Timestamp), cancellationToken);
        var dtos = new List<InventoryTransactionResponse>();
        foreach (var t in items) dtos.Add(await MapToResponseAsync(t, cancellationToken));
        return PagedResult<InventoryTransactionResponse>.From(dtos, totalCount, request);
    }

    public async Task<InventoryTransactionResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.InventoryTransactions.GetByIdAsync(id, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    public async Task<PagedResult<InventoryTransactionResponse>> GetByProductAsync(Guid productId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.InventoryTransactions.GetPagedAsync(request, filter: t => t.ProductId == productId, orderBy: q => q.OrderByDescending(t => t.Timestamp), cancellationToken);
        var dtos = new List<InventoryTransactionResponse>();
        foreach (var t in items) dtos.Add(await MapToResponseAsync(t, cancellationToken));
        return PagedResult<InventoryTransactionResponse>.From(dtos, totalCount, request);
    }

    public async Task<PagedResult<InventoryTransactionResponse>> GetByZoneAsync(Guid zoneId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.InventoryTransactions.GetPagedAsync(request, 
            filter: t => t.OriginZoneId == zoneId || t.DestinationZoneId == zoneId, 
            orderBy: q => q.OrderByDescending(t => t.Timestamp), cancellationToken);
        var dtos = new List<InventoryTransactionResponse>();
        foreach (var t in items) dtos.Add(await MapToResponseAsync(t, cancellationToken));
        return PagedResult<InventoryTransactionResponse>.From(dtos, totalCount, request);
    }

    public async Task<PagedResult<InventoryTransactionResponse>> GetByTypeAsync(Guid tenantId, InventoryTransactionType type, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.InventoryTransactions.GetPagedAsync(request, 
            filter: t => t.TenantId == tenantId && t.TransactionType == type, 
            orderBy: q => q.OrderByDescending(t => t.Timestamp), cancellationToken);
        var dtos = new List<InventoryTransactionResponse>();
        foreach (var t in items) dtos.Add(await MapToResponseAsync(t, cancellationToken));
        return PagedResult<InventoryTransactionResponse>.From(dtos, totalCount, request);
    }

    public async Task<OperationResult<InventoryTransactionResponse>> RecordReceiptAsync(CreateInventoryTransactionRequest request, Guid performedByUserId, CancellationToken cancellationToken = default)
    {
        return await CreateTransactionAsync(request, performedByUserId, InventoryTransactionType.Receipt, cancellationToken);
    }

    public async Task<OperationResult<InventoryTransactionResponse>> RecordDispatchAsync(CreateInventoryTransactionRequest request, Guid performedByUserId, CancellationToken cancellationToken = default)
    {
        return await CreateTransactionAsync(request, performedByUserId, InventoryTransactionType.Dispatch, cancellationToken);
    }

    public async Task<OperationResult<InventoryTransactionResponse>> RecordTransferAsync(CreateInventoryTransactionRequest request, Guid performedByUserId, CancellationToken cancellationToken = default)
    {
        if (!request.OriginZoneId.HasValue || !request.DestinationZoneId.HasValue)
            return OperationResult<InventoryTransactionResponse>.Fail("Transferencia requiere zona origen y destino");

        return await CreateTransactionAsync(request, performedByUserId, InventoryTransactionType.InternalMove, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _unitOfWork.InventoryTransactions.AnyAsync(t => t.Id == id, cancellationToken);

    private async Task<OperationResult<InventoryTransactionResponse>> CreateTransactionAsync(
        CreateInventoryTransactionRequest request, 
        Guid performedByUserId, 
        InventoryTransactionType type, 
        CancellationToken cancellationToken)
    {
        // Get tenant from zone
        Guid tenantId = Guid.Empty;
        if (request.DestinationZoneId.HasValue)
        {
            var zone = await _unitOfWork.WarehouseZones.GetByIdAsync(request.DestinationZoneId.Value, cancellationToken);
            if (zone != null)
            {
                var location = await _unitOfWork.Locations.GetByIdAsync(zone.LocationId, cancellationToken);
                tenantId = location?.TenantId ?? Guid.Empty;
            }
        }
        else if (request.OriginZoneId.HasValue)
        {
            var zone = await _unitOfWork.WarehouseZones.GetByIdAsync(request.OriginZoneId.Value, cancellationToken);
            if (zone != null)
            {
                var location = await _unitOfWork.Locations.GetByIdAsync(zone.LocationId, cancellationToken);
                tenantId = location?.TenantId ?? Guid.Empty;
            }
        }

        var entity = new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductId = request.ProductId,
            OriginZoneId = request.OriginZoneId,
            DestinationZoneId = request.DestinationZoneId,
            Quantity = request.Quantity,
            TransactionType = type,
            PerformedByUserId = performedByUserId,
            ShipmentId = request.ShipmentId,
            BatchNumber = request.BatchNumber,
            Remarks = request.Remarks,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.InventoryTransactions.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<InventoryTransactionResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), $"Transacción {type} registrada exitosamente");
    }

    private async Task<InventoryTransactionResponse> MapToResponseAsync(InventoryTransaction t, CancellationToken ct)
    {
        var product = await _unitOfWork.CatalogItems.GetByIdAsync(t.ProductId, ct);
        var originZone = t.OriginZoneId.HasValue ? await _unitOfWork.WarehouseZones.GetByIdAsync(t.OriginZoneId.Value, ct) : null;
        var destZone = t.DestinationZoneId.HasValue ? await _unitOfWork.WarehouseZones.GetByIdAsync(t.DestinationZoneId.Value, ct) : null;
        var user = await _unitOfWork.Users.GetByIdAsync(t.PerformedByUserId, ct);

        return new InventoryTransactionResponse(t.Id, t.ProductId, product?.Name ?? "", t.OriginZoneId, originZone?.Name, t.DestinationZoneId, destZone?.Name,
            t.Quantity, t.TransactionType.ToString(), t.PerformedByUserId, user?.FullName ?? "", t.ShipmentId, t.BatchNumber, t.Remarks, t.Timestamp, t.CreatedAt);
    }
}
