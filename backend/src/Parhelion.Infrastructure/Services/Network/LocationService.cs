using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Services.Network;

/// <summary>
/// Implementación del servicio de Locations.
/// </summary>
public class LocationService : ILocationService
{
    private readonly IUnitOfWork _unitOfWork;

    public LocationService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<LocationResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Locations.GetPagedAsync(request, filter: null, orderBy: q => q.OrderBy(l => l.Name), cancellationToken);
        return PagedResult<LocationResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<LocationResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Locations.GetByIdAsync(id, cancellationToken);
        return entity != null ? MapToResponse(entity) : null;
    }

    public async Task<OperationResult<LocationResponse>> CreateAsync(CreateLocationRequest request, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<LocationType>(request.Type, out var locationType))
            return OperationResult<LocationResponse>.Fail("Tipo de ubicación inválido");

        var entity = new Location
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Type = locationType,
            FullAddress = request.FullAddress,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CanReceive = request.CanReceive,
            CanDispatch = request.CanDispatch,
            IsInternal = request.IsInternal,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Locations.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<LocationResponse>.Ok(MapToResponse(entity), "Ubicación creada exitosamente");
    }

    public async Task<OperationResult<LocationResponse>> UpdateAsync(Guid id, UpdateLocationRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Locations.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult<LocationResponse>.Fail("Ubicación no encontrada");

        if (!Enum.TryParse<LocationType>(request.Type, out var locationType))
            return OperationResult<LocationResponse>.Fail("Tipo de ubicación inválido");

        entity.Code = request.Code;
        entity.Name = request.Name;
        entity.Type = locationType;
        entity.FullAddress = request.FullAddress;
        entity.Latitude = request.Latitude;
        entity.Longitude = request.Longitude;
        entity.CanReceive = request.CanReceive;
        entity.CanDispatch = request.CanDispatch;
        entity.IsInternal = request.IsInternal;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Locations.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<LocationResponse>.Ok(MapToResponse(entity), "Ubicación actualizada exitosamente");
    }

    public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Locations.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult.Fail("Ubicación no encontrada");
        _unitOfWork.Locations.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Ubicación eliminada exitosamente");
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _unitOfWork.Locations.AnyAsync(l => l.Id == id, cancellationToken);

    public async Task<PagedResult<LocationResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Locations.GetPagedAsync(request, filter: l => l.TenantId == tenantId, orderBy: q => q.OrderBy(l => l.Name), cancellationToken);
        return PagedResult<LocationResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<PagedResult<LocationResponse>> GetByTypeAsync(Guid tenantId, LocationType locationType, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Locations.GetPagedAsync(request, filter: l => l.TenantId == tenantId && l.Type == locationType, orderBy: q => q.OrderBy(l => l.Name), cancellationToken);
        return PagedResult<LocationResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<PagedResult<LocationResponse>> GetActiveAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Locations.GetPagedAsync(request, filter: l => l.TenantId == tenantId && l.IsActive, orderBy: q => q.OrderBy(l => l.Name), cancellationToken);
        return PagedResult<LocationResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<PagedResult<LocationResponse>> SearchByNameAsync(Guid tenantId, string name, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var term = name.ToLower();
        var (items, totalCount) = await _unitOfWork.Locations.GetPagedAsync(request, filter: l => l.TenantId == tenantId && (l.Name.ToLower().Contains(term) || l.Code.ToLower().Contains(term)), orderBy: q => q.OrderBy(l => l.Name), cancellationToken);
        return PagedResult<LocationResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    private static LocationResponse MapToResponse(Location e) => new(e.Id, e.Code, e.Name, e.Type.ToString(), e.FullAddress, e.Latitude, e.Longitude, e.CanReceive, e.CanDispatch, e.IsInternal, e.IsActive, e.CreatedAt, e.UpdatedAt);
}
