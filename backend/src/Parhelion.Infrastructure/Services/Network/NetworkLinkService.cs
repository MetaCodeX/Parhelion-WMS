using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Network;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Services.Network;

/// <summary>
/// Implementación del servicio de enlaces de red logística.
/// </summary>
public class NetworkLinkService : INetworkLinkService
{
    private readonly IUnitOfWork _unitOfWork;

    public NetworkLinkService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<NetworkLinkResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.NetworkLinks.GetPagedAsync(request, filter: null, orderBy: q => q.OrderBy(l => l.CreatedAt), cancellationToken);
        var dtos = new List<NetworkLinkResponse>();
        foreach (var l in items) dtos.Add(await MapToResponseAsync(l, cancellationToken));
        return PagedResult<NetworkLinkResponse>.From(dtos, totalCount, request);
    }

    public async Task<NetworkLinkResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.NetworkLinks.GetByIdAsync(id, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    public async Task<OperationResult<NetworkLinkResponse>> CreateAsync(CreateNetworkLinkRequest request, CancellationToken cancellationToken = default)
    {
        // Get tenant from origin location
        var originLocation = await _unitOfWork.Locations.GetByIdAsync(request.OriginLocationId, cancellationToken);
        if (originLocation == null) return OperationResult<NetworkLinkResponse>.Fail("Ubicación origen no encontrada");

        var entity = new NetworkLink
        {
            Id = Guid.NewGuid(),
            TenantId = originLocation.TenantId,
            OriginLocationId = request.OriginLocationId,
            DestinationLocationId = request.DestinationLocationId,
            LinkType = Enum.TryParse<NetworkLinkType>(request.LinkType, out var lt) ? lt : NetworkLinkType.LineHaul,
            TransitTime = request.TransitTime,
            IsBidirectional = request.IsBidirectional,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.NetworkLinks.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<NetworkLinkResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Enlace creado exitosamente");
    }

    public async Task<OperationResult<NetworkLinkResponse>> UpdateAsync(Guid id, UpdateNetworkLinkRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.NetworkLinks.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult<NetworkLinkResponse>.Fail("Enlace no encontrado");

        if (Enum.TryParse<NetworkLinkType>(request.LinkType, out var lt)) entity.LinkType = lt;
        entity.TransitTime = request.TransitTime;
        entity.IsBidirectional = request.IsBidirectional;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.NetworkLinks.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<NetworkLinkResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Enlace actualizado exitosamente");
    }

    public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.NetworkLinks.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult.Fail("Enlace no encontrado");
        _unitOfWork.NetworkLinks.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Enlace eliminado exitosamente");
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _unitOfWork.NetworkLinks.AnyAsync(l => l.Id == id, cancellationToken);

    public async Task<PagedResult<NetworkLinkResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.NetworkLinks.GetPagedAsync(request, filter: l => l.TenantId == tenantId, orderBy: q => q.OrderBy(l => l.CreatedAt), cancellationToken);
        var dtos = new List<NetworkLinkResponse>();
        foreach (var l in items) dtos.Add(await MapToResponseAsync(l, cancellationToken));
        return PagedResult<NetworkLinkResponse>.From(dtos, totalCount, request);
    }

    public async Task<PagedResult<NetworkLinkResponse>> GetByOriginAsync(Guid originLocationId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.NetworkLinks.GetPagedAsync(request, filter: l => l.OriginLocationId == originLocationId, orderBy: q => q.OrderBy(l => l.CreatedAt), cancellationToken);
        var dtos = new List<NetworkLinkResponse>();
        foreach (var l in items) dtos.Add(await MapToResponseAsync(l, cancellationToken));
        return PagedResult<NetworkLinkResponse>.From(dtos, totalCount, request);
    }

    public async Task<PagedResult<NetworkLinkResponse>> GetByDestinationAsync(Guid destinationLocationId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.NetworkLinks.GetPagedAsync(request, filter: l => l.DestinationLocationId == destinationLocationId, orderBy: q => q.OrderBy(l => l.CreatedAt), cancellationToken);
        var dtos = new List<NetworkLinkResponse>();
        foreach (var l in items) dtos.Add(await MapToResponseAsync(l, cancellationToken));
        return PagedResult<NetworkLinkResponse>.From(dtos, totalCount, request);
    }

    public async Task<PagedResult<NetworkLinkResponse>> GetActiveAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.NetworkLinks.GetPagedAsync(request, filter: l => l.TenantId == tenantId && l.IsActive, orderBy: q => q.OrderBy(l => l.CreatedAt), cancellationToken);
        var dtos = new List<NetworkLinkResponse>();
        foreach (var l in items) dtos.Add(await MapToResponseAsync(l, cancellationToken));
        return PagedResult<NetworkLinkResponse>.From(dtos, totalCount, request);
    }

    private async Task<NetworkLinkResponse> MapToResponseAsync(NetworkLink l, CancellationToken ct)
    {
        var origin = await _unitOfWork.Locations.GetByIdAsync(l.OriginLocationId, ct);
        var destination = await _unitOfWork.Locations.GetByIdAsync(l.DestinationLocationId, ct);
        return new NetworkLinkResponse(l.Id, l.OriginLocationId, origin?.Name ?? "", l.DestinationLocationId, destination?.Name ?? "", 
            l.LinkType.ToString(), l.TransitTime, l.IsBidirectional, l.IsActive, l.CreatedAt, l.UpdatedAt);
    }
}
