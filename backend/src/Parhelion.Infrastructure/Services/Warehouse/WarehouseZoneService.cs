using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Services.Warehouse;

/// <summary>
/// Implementación del servicio de zonas de almacén.
/// </summary>
public class WarehouseZoneService : IWarehouseZoneService
{
    private readonly IUnitOfWork _unitOfWork;

    public WarehouseZoneService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<WarehouseZoneResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.WarehouseZones.GetPagedAsync(request, filter: null, orderBy: q => q.OrderBy(z => z.Code), cancellationToken);
        var dtos = new List<WarehouseZoneResponse>();
        foreach (var z in items) dtos.Add(await MapToResponseAsync(z, cancellationToken));
        return PagedResult<WarehouseZoneResponse>.From(dtos, totalCount, request);
    }

    public async Task<WarehouseZoneResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.WarehouseZones.GetByIdAsync(id, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    public async Task<OperationResult<WarehouseZoneResponse>> CreateAsync(CreateWarehouseZoneRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new WarehouseZone
        {
            Id = Guid.NewGuid(),
            LocationId = request.LocationId,
            Code = request.Code,
            Name = request.Name,
            Type = Enum.TryParse<WarehouseZoneType>(request.Type, out var t) ? t : WarehouseZoneType.Storage,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.WarehouseZones.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<WarehouseZoneResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Zona creada exitosamente");
    }

    public async Task<OperationResult<WarehouseZoneResponse>> UpdateAsync(Guid id, UpdateWarehouseZoneRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.WarehouseZones.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult<WarehouseZoneResponse>.Fail("Zona no encontrada");

        entity.Code = request.Code;
        entity.Name = request.Name;
        if (Enum.TryParse<WarehouseZoneType>(request.Type, out var t)) entity.Type = t;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.WarehouseZones.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<WarehouseZoneResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Zona actualizada exitosamente");
    }

    public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.WarehouseZones.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult.Fail("Zona no encontrada");
        _unitOfWork.WarehouseZones.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Zona eliminada exitosamente");
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _unitOfWork.WarehouseZones.AnyAsync(z => z.Id == id, cancellationToken);

    public async Task<PagedResult<WarehouseZoneResponse>> GetByLocationAsync(Guid locationId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.WarehouseZones.GetPagedAsync(request, filter: z => z.LocationId == locationId, orderBy: q => q.OrderBy(z => z.Code), cancellationToken);
        var dtos = new List<WarehouseZoneResponse>();
        foreach (var z in items) dtos.Add(await MapToResponseAsync(z, cancellationToken));
        return PagedResult<WarehouseZoneResponse>.From(dtos, totalCount, request);
    }

    public async Task<PagedResult<WarehouseZoneResponse>> GetActiveAsync(Guid locationId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.WarehouseZones.GetPagedAsync(request, filter: z => z.LocationId == locationId && z.IsActive, orderBy: q => q.OrderBy(z => z.Code), cancellationToken);
        var dtos = new List<WarehouseZoneResponse>();
        foreach (var z in items) dtos.Add(await MapToResponseAsync(z, cancellationToken));
        return PagedResult<WarehouseZoneResponse>.From(dtos, totalCount, request);
    }

    public async Task<WarehouseZoneResponse?> GetByCodeAsync(Guid locationId, string code, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.WarehouseZones.FirstOrDefaultAsync(z => z.LocationId == locationId && z.Code == code, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    private async Task<WarehouseZoneResponse> MapToResponseAsync(WarehouseZone z, CancellationToken ct)
    {
        var location = await _unitOfWork.Locations.GetByIdAsync(z.LocationId, ct);
        return new WarehouseZoneResponse(z.Id, z.LocationId, location?.Name ?? "", z.Code, z.Name, z.Type.ToString(), z.IsActive, z.CreatedAt, z.UpdatedAt);
    }
}
