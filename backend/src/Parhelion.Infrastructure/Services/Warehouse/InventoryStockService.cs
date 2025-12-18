using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Services.Warehouse;

/// <summary>
/// Implementación del servicio de stock de inventario.
/// </summary>
public class InventoryStockService : IInventoryStockService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryStockService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<InventoryStockResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.InventoryStocks.GetPagedAsync(request, filter: null, orderBy: q => q.OrderBy(s => s.CreatedAt), cancellationToken);
        var dtos = new List<InventoryStockResponse>();
        foreach (var s in items) dtos.Add(await MapToResponseAsync(s, cancellationToken));
        return PagedResult<InventoryStockResponse>.From(dtos, totalCount, request);
    }

    public async Task<InventoryStockResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.InventoryStocks.GetByIdAsync(id, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    public async Task<OperationResult<InventoryStockResponse>> CreateAsync(CreateInventoryStockRequest request, CancellationToken cancellationToken = default)
    {
        var zone = await _unitOfWork.WarehouseZones.GetByIdAsync(request.ZoneId, cancellationToken);
        if (zone == null) return OperationResult<InventoryStockResponse>.Fail("Zona no encontrada");

        var location = await _unitOfWork.Locations.GetByIdAsync(zone.LocationId, cancellationToken);
        
        var entity = new InventoryStock
        {
            Id = Guid.NewGuid(),
            TenantId = location?.TenantId ?? Guid.Empty,
            ZoneId = request.ZoneId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            QuantityReserved = request.QuantityReserved,
            BatchNumber = request.BatchNumber,
            ExpiryDate = request.ExpiryDate,
            UnitCost = request.UnitCost,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.InventoryStocks.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<InventoryStockResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Stock creado exitosamente");
    }

    public async Task<OperationResult<InventoryStockResponse>> UpdateAsync(Guid id, UpdateInventoryStockRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.InventoryStocks.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult<InventoryStockResponse>.Fail("Stock no encontrado");

        entity.Quantity = request.Quantity;
        entity.QuantityReserved = request.QuantityReserved;
        entity.BatchNumber = request.BatchNumber;
        entity.ExpiryDate = request.ExpiryDate;
        entity.LastCountDate = request.LastCountDate;
        entity.UnitCost = request.UnitCost;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.InventoryStocks.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<InventoryStockResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Stock actualizado exitosamente");
    }

    public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.InventoryStocks.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult.Fail("Stock no encontrado");
        _unitOfWork.InventoryStocks.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Stock eliminado exitosamente");
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _unitOfWork.InventoryStocks.AnyAsync(s => s.Id == id, cancellationToken);

    public async Task<PagedResult<InventoryStockResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.InventoryStocks.GetPagedAsync(request, filter: s => s.TenantId == tenantId, orderBy: q => q.OrderBy(s => s.CreatedAt), cancellationToken);
        var dtos = new List<InventoryStockResponse>();
        foreach (var s in items) dtos.Add(await MapToResponseAsync(s, cancellationToken));
        return PagedResult<InventoryStockResponse>.From(dtos, totalCount, request);
    }

    public async Task<PagedResult<InventoryStockResponse>> GetByZoneAsync(Guid zoneId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.InventoryStocks.GetPagedAsync(request, filter: s => s.ZoneId == zoneId, orderBy: q => q.OrderBy(s => s.CreatedAt), cancellationToken);
        var dtos = new List<InventoryStockResponse>();
        foreach (var s in items) dtos.Add(await MapToResponseAsync(s, cancellationToken));
        return PagedResult<InventoryStockResponse>.From(dtos, totalCount, request);
    }

    public async Task<PagedResult<InventoryStockResponse>> GetByProductAsync(Guid productId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.InventoryStocks.GetPagedAsync(request, filter: s => s.ProductId == productId, orderBy: q => q.OrderBy(s => s.CreatedAt), cancellationToken);
        var dtos = new List<InventoryStockResponse>();
        foreach (var s in items) dtos.Add(await MapToResponseAsync(s, cancellationToken));
        return PagedResult<InventoryStockResponse>.From(dtos, totalCount, request);
    }

    public async Task<PagedResult<InventoryStockResponse>> GetLowStockAsync(Guid tenantId, decimal threshold, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.InventoryStocks.GetPagedAsync(request, 
            filter: s => s.TenantId == tenantId && s.QuantityAvailable <= threshold, 
            orderBy: q => q.OrderBy(s => s.QuantityAvailable), cancellationToken);
        var dtos = new List<InventoryStockResponse>();
        foreach (var s in items) dtos.Add(await MapToResponseAsync(s, cancellationToken));
        return PagedResult<InventoryStockResponse>.From(dtos, totalCount, request);
    }

    public async Task<OperationResult<InventoryStockResponse>> ReserveQuantityAsync(Guid stockId, decimal quantity, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.InventoryStocks.GetByIdAsync(stockId, cancellationToken);
        if (entity == null) return OperationResult<InventoryStockResponse>.Fail("Stock no encontrado");
        
        if (entity.QuantityAvailable < quantity)
            return OperationResult<InventoryStockResponse>.Fail($"Cantidad insuficiente. Disponible: {entity.QuantityAvailable}");

        entity.QuantityReserved += quantity;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.InventoryStocks.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<InventoryStockResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), $"Reservados {quantity} unidades");
    }

    public async Task<OperationResult<InventoryStockResponse>> ReleaseReservedAsync(Guid stockId, decimal quantity, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.InventoryStocks.GetByIdAsync(stockId, cancellationToken);
        if (entity == null) return OperationResult<InventoryStockResponse>.Fail("Stock no encontrado");
        
        if (entity.QuantityReserved < quantity)
            return OperationResult<InventoryStockResponse>.Fail($"Cantidad reservada insuficiente: {entity.QuantityReserved}");

        entity.QuantityReserved -= quantity;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.InventoryStocks.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<InventoryStockResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), $"Liberadas {quantity} unidades");
    }

    private async Task<InventoryStockResponse> MapToResponseAsync(InventoryStock s, CancellationToken ct)
    {
        var zone = await _unitOfWork.WarehouseZones.GetByIdAsync(s.ZoneId, ct);
        var product = await _unitOfWork.CatalogItems.GetByIdAsync(s.ProductId, ct);
        
        return new InventoryStockResponse(s.Id, s.ZoneId, zone?.Name ?? "", s.ProductId, product?.Name ?? "", product?.Sku ?? "", 
            s.Quantity, s.QuantityReserved, s.QuantityAvailable, s.BatchNumber, s.ExpiryDate, s.LastCountDate, s.UnitCost, s.CreatedAt, s.UpdatedAt);
    }
}
