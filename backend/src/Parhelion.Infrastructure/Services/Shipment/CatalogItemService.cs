using Parhelion.Application.DTOs.Catalog;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Services.Shipment;

/// <summary>
/// Implementación del servicio de CatalogItems.
/// </summary>
public class CatalogItemService : ICatalogItemService
{
    private readonly IUnitOfWork _unitOfWork;

    public CatalogItemService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<CatalogItemResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.CatalogItems.GetPagedAsync(request, filter: null, orderBy: q => q.OrderBy(c => c.Name), cancellationToken);
        return PagedResult<CatalogItemResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<CatalogItemResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.CatalogItems.GetByIdAsync(id, cancellationToken);
        return entity != null ? MapToResponse(entity) : null;
    }

    public async Task<OperationResult<CatalogItemResponse>> CreateAsync(CreateCatalogItemRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.CatalogItems.FirstOrDefaultAsync(c => c.Sku == request.Sku, cancellationToken);
        if (existing != null) return OperationResult<CatalogItemResponse>.Fail($"Ya existe un producto con SKU '{request.Sku}'");

        var entity = new CatalogItem
        {
            Id = Guid.NewGuid(),
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            BaseUom = request.BaseUom,
            DefaultWeightKg = request.DefaultWeightKg,
            DefaultWidthCm = request.DefaultWidthCm,
            DefaultHeightCm = request.DefaultHeightCm,
            DefaultLengthCm = request.DefaultLengthCm,
            RequiresRefrigeration = request.RequiresRefrigeration,
            IsHazardous = request.IsHazardous,
            IsFragile = request.IsFragile,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.CatalogItems.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<CatalogItemResponse>.Ok(MapToResponse(entity), "Producto creado exitosamente");
    }

    public async Task<OperationResult<CatalogItemResponse>> UpdateAsync(Guid id, UpdateCatalogItemRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.CatalogItems.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult<CatalogItemResponse>.Fail("Producto no encontrado");

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.BaseUom = request.BaseUom;
        entity.DefaultWeightKg = request.DefaultWeightKg;
        entity.DefaultWidthCm = request.DefaultWidthCm;
        entity.DefaultHeightCm = request.DefaultHeightCm;
        entity.DefaultLengthCm = request.DefaultLengthCm;
        entity.RequiresRefrigeration = request.RequiresRefrigeration;
        entity.IsHazardous = request.IsHazardous;
        entity.IsFragile = request.IsFragile;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.CatalogItems.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<CatalogItemResponse>.Ok(MapToResponse(entity), "Producto actualizado exitosamente");
    }

    public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.CatalogItems.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult.Fail("Producto no encontrado");
        _unitOfWork.CatalogItems.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Producto eliminado exitosamente");
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _unitOfWork.CatalogItems.AnyAsync(c => c.Id == id, cancellationToken);

    public async Task<CatalogItemResponse?> GetBySkuAsync(Guid tenantId, string sku, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.CatalogItems.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Sku == sku, cancellationToken);
        return entity != null ? MapToResponse(entity) : null;
    }

    public async Task<PagedResult<CatalogItemResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.CatalogItems.GetPagedAsync(request, filter: c => c.TenantId == tenantId, orderBy: q => q.OrderBy(c => c.Name), cancellationToken);
        return PagedResult<CatalogItemResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<PagedResult<CatalogItemResponse>> SearchAsync(Guid tenantId, string searchTerm, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        var (items, totalCount) = await _unitOfWork.CatalogItems.GetPagedAsync(request, filter: c => c.TenantId == tenantId && (c.Name.ToLower().Contains(term) || c.Sku.ToLower().Contains(term) || (c.Description != null && c.Description.ToLower().Contains(term))), orderBy: q => q.OrderBy(c => c.Name), cancellationToken);
        return PagedResult<CatalogItemResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<PagedResult<CatalogItemResponse>> GetRefrigeratedAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.CatalogItems.GetPagedAsync(request, filter: c => c.TenantId == tenantId && c.RequiresRefrigeration, orderBy: q => q.OrderBy(c => c.Name), cancellationToken);
        return PagedResult<CatalogItemResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<PagedResult<CatalogItemResponse>> GetHazardousAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.CatalogItems.GetPagedAsync(request, filter: c => c.TenantId == tenantId && c.IsHazardous, orderBy: q => q.OrderBy(c => c.Name), cancellationToken);
        return PagedResult<CatalogItemResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    private static CatalogItemResponse MapToResponse(CatalogItem e) => new(
        e.Id, e.Sku, e.Name, e.Description, e.BaseUom, e.DefaultWeightKg, e.DefaultWidthCm, e.DefaultHeightCm, e.DefaultLengthCm,
        e.DefaultVolumeM3, e.RequiresRefrigeration, e.IsHazardous, e.IsFragile, e.IsActive, e.CreatedAt, e.UpdatedAt);
}
