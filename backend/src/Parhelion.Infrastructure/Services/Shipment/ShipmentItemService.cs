using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Services.Shipment;

/// <summary>
/// Implementación del servicio de ShipmentItems.
/// </summary>
public class ShipmentItemService : IShipmentItemService
{
    private readonly IUnitOfWork _unitOfWork;

    public ShipmentItemService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<ShipmentItemResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.ShipmentItems.GetPagedAsync(request, filter: null, orderBy: q => q.OrderByDescending(i => i.CreatedAt), cancellationToken);
        var dtos = new List<ShipmentItemResponse>();
        foreach (var i in items) dtos.Add(await MapToResponseAsync(i, cancellationToken));
        return PagedResult<ShipmentItemResponse>.From(dtos, totalCount, request);
    }

    public async Task<ShipmentItemResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.ShipmentItems.GetByIdAsync(id, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    public async Task<OperationResult<ShipmentItemResponse>> CreateAsync(CreateShipmentItemRequest request, CancellationToken cancellationToken = default)
    {
        var shipment = await _unitOfWork.Shipments.GetByIdAsync(request.ShipmentId, cancellationToken);
        if (shipment == null) return OperationResult<ShipmentItemResponse>.Fail("Envío no encontrado");

        if (!Enum.TryParse<PackagingType>(request.PackagingType, out var packagingType))
            return OperationResult<ShipmentItemResponse>.Fail("Tipo de empaque inválido");

        var entity = new ShipmentItem
        {
            Id = Guid.NewGuid(),
            ShipmentId = request.ShipmentId,
            ProductId = request.ProductId,
            Sku = request.Sku,
            Description = request.Description,
            PackagingType = packagingType,
            Quantity = request.Quantity,
            WeightKg = request.WeightKg,
            WidthCm = request.WidthCm,
            HeightCm = request.HeightCm,
            LengthCm = request.LengthCm,
            DeclaredValue = request.DeclaredValue,
            IsFragile = request.IsFragile,
            IsHazardous = request.IsHazardous,
            RequiresRefrigeration = request.RequiresRefrigeration,
            StackingInstructions = request.StackingInstructions,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ShipmentItems.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<ShipmentItemResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Item creado exitosamente");
    }

    public async Task<OperationResult<ShipmentItemResponse>> UpdateAsync(Guid id, UpdateShipmentItemRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.ShipmentItems.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult<ShipmentItemResponse>.Fail("Item no encontrado");

        if (!Enum.TryParse<PackagingType>(request.PackagingType, out var packagingType))
            return OperationResult<ShipmentItemResponse>.Fail("Tipo de empaque inválido");

        entity.Sku = request.Sku;
        entity.Description = request.Description;
        entity.PackagingType = packagingType;
        entity.Quantity = request.Quantity;
        entity.WeightKg = request.WeightKg;
        entity.WidthCm = request.WidthCm;
        entity.HeightCm = request.HeightCm;
        entity.LengthCm = request.LengthCm;
        entity.DeclaredValue = request.DeclaredValue;
        entity.IsFragile = request.IsFragile;
        entity.IsHazardous = request.IsHazardous;
        entity.RequiresRefrigeration = request.RequiresRefrigeration;
        entity.StackingInstructions = request.StackingInstructions;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.ShipmentItems.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<ShipmentItemResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Item actualizado exitosamente");
    }

    public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.ShipmentItems.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult.Fail("Item no encontrado");
        _unitOfWork.ShipmentItems.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Item eliminado exitosamente");
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _unitOfWork.ShipmentItems.AnyAsync(i => i.Id == id, cancellationToken);

    public async Task<PagedResult<ShipmentItemResponse>> GetByShipmentAsync(Guid shipmentId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.ShipmentItems.GetPagedAsync(request, filter: i => i.ShipmentId == shipmentId, orderBy: q => q.OrderBy(i => i.CreatedAt), cancellationToken);
        var dtos = new List<ShipmentItemResponse>();
        foreach (var i in items) dtos.Add(await MapToResponseAsync(i, cancellationToken));
        return PagedResult<ShipmentItemResponse>.From(dtos, totalCount, request);
    }

    public decimal CalculateVolumetricWeight(decimal widthCm, decimal heightCm, decimal lengthCm, decimal factorDimensional = 5000) =>
        (widthCm * heightCm * lengthCm) / factorDimensional;

    public async Task<IEnumerable<ShipmentItemResponse>> GetRefrigeratedItemsAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.ShipmentItems.FindAsync(i => i.ShipmentId == shipmentId && i.RequiresRefrigeration, cancellationToken);
        var dtos = new List<ShipmentItemResponse>();
        foreach (var i in items) dtos.Add(await MapToResponseAsync(i, cancellationToken));
        return dtos;
    }

    public async Task<IEnumerable<ShipmentItemResponse>> GetHazardousItemsAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.ShipmentItems.FindAsync(i => i.ShipmentId == shipmentId && i.IsHazardous, cancellationToken);
        var dtos = new List<ShipmentItemResponse>();
        foreach (var i in items) dtos.Add(await MapToResponseAsync(i, cancellationToken));
        return dtos;
    }

    private async Task<ShipmentItemResponse> MapToResponseAsync(ShipmentItem e, CancellationToken ct)
    {
        var product = e.ProductId.HasValue ? await _unitOfWork.CatalogItems.GetByIdAsync(e.ProductId.Value, ct) : null;
        return new ShipmentItemResponse(e.Id, e.ShipmentId, e.ProductId, product?.Name, e.Sku, e.Description,
            e.PackagingType.ToString(), e.Quantity, e.WeightKg, e.WidthCm, e.HeightCm, e.LengthCm,
            e.VolumeM3, e.VolumetricWeightKg, e.DeclaredValue, e.IsFragile, e.IsHazardous,
            e.RequiresRefrigeration, e.StackingInstructions, e.CreatedAt, e.UpdatedAt);
    }
}
