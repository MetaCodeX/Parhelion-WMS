using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Network;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Services.Network;

/// <summary>
/// Implementación del servicio de Routes.
/// </summary>
public class RouteService : IRouteService
{
    private readonly IUnitOfWork _unitOfWork;

    public RouteService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<RouteBlueprintResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.RouteBlueprints.GetPagedAsync(request, filter: null, orderBy: q => q.OrderBy(r => r.Name), cancellationToken);
        return PagedResult<RouteBlueprintResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<RouteBlueprintResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.RouteBlueprints.GetByIdAsync(id, cancellationToken);
        return entity != null ? MapToResponse(entity) : null;
    }

    public async Task<OperationResult<RouteBlueprintResponse>> CreateAsync(CreateRouteBlueprintRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new RouteBlueprint
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            TotalSteps = 0,
            TotalTransitTime = TimeSpan.Zero,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RouteBlueprints.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<RouteBlueprintResponse>.Ok(MapToResponse(entity), "Ruta creada exitosamente");
    }

    public async Task<OperationResult<RouteBlueprintResponse>> UpdateAsync(Guid id, UpdateRouteBlueprintRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.RouteBlueprints.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult<RouteBlueprintResponse>.Fail("Ruta no encontrada");

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.TotalSteps = request.TotalSteps;
        entity.TotalTransitTime = request.TotalTransitTime;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.RouteBlueprints.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<RouteBlueprintResponse>.Ok(MapToResponse(entity), "Ruta actualizada exitosamente");
    }

    public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.RouteBlueprints.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult.Fail("Ruta no encontrada");
        _unitOfWork.RouteBlueprints.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Ruta eliminada exitosamente");
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _unitOfWork.RouteBlueprints.AnyAsync(r => r.Id == id, cancellationToken);

    public async Task<PagedResult<RouteBlueprintResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.RouteBlueprints.GetPagedAsync(request, filter: r => r.TenantId == tenantId, orderBy: q => q.OrderBy(r => r.Name), cancellationToken);
        return PagedResult<RouteBlueprintResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<PagedResult<RouteBlueprintResponse>> GetActiveAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.RouteBlueprints.GetPagedAsync(request, filter: r => r.TenantId == tenantId && r.IsActive, orderBy: q => q.OrderBy(r => r.Name), cancellationToken);
        return PagedResult<RouteBlueprintResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<PagedResult<RouteBlueprintResponse>> SearchByNameAsync(Guid tenantId, string name, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var term = name.ToLower();
        var (items, totalCount) = await _unitOfWork.RouteBlueprints.GetPagedAsync(request, filter: r => r.TenantId == tenantId && r.Name.ToLower().Contains(term), orderBy: q => q.OrderBy(r => r.Name), cancellationToken);
        return PagedResult<RouteBlueprintResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<IEnumerable<RouteStepResponse>> GetStepsAsync(Guid routeId, CancellationToken cancellationToken = default)
    {
        var steps = await _unitOfWork.RouteSteps.FindAsync(s => s.RouteBlueprintId == routeId, cancellationToken);
        var orderedSteps = steps.OrderBy(s => s.StepOrder).ToList();
        var dtos = new List<RouteStepResponse>();
        foreach (var step in orderedSteps)
        {
            var route = await _unitOfWork.RouteBlueprints.GetByIdAsync(step.RouteBlueprintId, cancellationToken);
            var location = await _unitOfWork.Locations.GetByIdAsync(step.LocationId, cancellationToken);
            dtos.Add(new RouteStepResponse(step.Id, step.RouteBlueprintId, route?.Name ?? "", step.LocationId, location?.Name ?? "", step.StepOrder, step.StandardTransitTime, step.StepType.ToString(), step.CreatedAt, step.UpdatedAt));
        }
        return dtos;
    }

    private static RouteBlueprintResponse MapToResponse(RouteBlueprint e) => new(e.Id, e.Name, e.Description, e.TotalSteps, e.TotalTransitTime, e.IsActive, e.CreatedAt, e.UpdatedAt);
}
