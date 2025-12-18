using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Network;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Services.Network;

/// <summary>
/// Implementación del servicio de pasos de ruta.
/// </summary>
public class RouteStepService : IRouteStepService
{
    private readonly IUnitOfWork _unitOfWork;

    public RouteStepService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<RouteStepResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.RouteSteps.GetPagedAsync(request, filter: null, orderBy: q => q.OrderBy(s => s.StepOrder), cancellationToken);
        var dtos = new List<RouteStepResponse>();
        foreach (var s in items) dtos.Add(await MapToResponseAsync(s, cancellationToken));
        return PagedResult<RouteStepResponse>.From(dtos, totalCount, request);
    }

    public async Task<RouteStepResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.RouteSteps.GetByIdAsync(id, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    public async Task<OperationResult<RouteStepResponse>> CreateAsync(CreateRouteStepRequest request, CancellationToken cancellationToken = default)
    {
        var route = await _unitOfWork.RouteBlueprints.GetByIdAsync(request.RouteBlueprintId, cancellationToken);
        if (route == null) return OperationResult<RouteStepResponse>.Fail("Ruta no encontrada");

        var entity = new RouteStep
        {
            Id = Guid.NewGuid(),
            RouteBlueprintId = request.RouteBlueprintId,
            LocationId = request.LocationId,
            StepOrder = request.StepOrder,
            StandardTransitTime = request.StandardTransitTime,
            StepType = Enum.TryParse<RouteStepType>(request.StepType, out var st) ? st : RouteStepType.Intermediate,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RouteSteps.AddAsync(entity, cancellationToken);
        
        // Update route totals
        route.TotalSteps += 1;
        route.TotalTransitTime += request.StandardTransitTime;
        route.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.RouteBlueprints.Update(route);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<RouteStepResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Paso creado exitosamente");
    }

    public async Task<OperationResult<RouteStepResponse>> UpdateAsync(Guid id, UpdateRouteStepRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.RouteSteps.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult<RouteStepResponse>.Fail("Paso no encontrado");

        var oldTransitTime = entity.StandardTransitTime;
        
        entity.StepOrder = request.StepOrder;
        entity.StandardTransitTime = request.StandardTransitTime;
        if (Enum.TryParse<RouteStepType>(request.StepType, out var st)) entity.StepType = st;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.RouteSteps.Update(entity);
        
        // Update route transit time
        var route = await _unitOfWork.RouteBlueprints.GetByIdAsync(entity.RouteBlueprintId, cancellationToken);
        if (route != null)
        {
            route.TotalTransitTime = route.TotalTransitTime - oldTransitTime + request.StandardTransitTime;
            route.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.RouteBlueprints.Update(route);
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<RouteStepResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Paso actualizado exitosamente");
    }

    public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.RouteSteps.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult.Fail("Paso no encontrado");
        
        // Update route totals
        var route = await _unitOfWork.RouteBlueprints.GetByIdAsync(entity.RouteBlueprintId, cancellationToken);
        if (route != null)
        {
            route.TotalSteps -= 1;
            route.TotalTransitTime -= entity.StandardTransitTime;
            route.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.RouteBlueprints.Update(route);
        }
        
        _unitOfWork.RouteSteps.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Paso eliminado exitosamente");
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _unitOfWork.RouteSteps.AnyAsync(s => s.Id == id, cancellationToken);

    public async Task<IEnumerable<RouteStepResponse>> GetByRouteAsync(Guid routeBlueprintId, CancellationToken cancellationToken = default)
    {
        var steps = await _unitOfWork.RouteSteps.FindAsync(s => s.RouteBlueprintId == routeBlueprintId, cancellationToken);
        var orderedSteps = steps.OrderBy(s => s.StepOrder).ToList();
        var dtos = new List<RouteStepResponse>();
        foreach (var s in orderedSteps) dtos.Add(await MapToResponseAsync(s, cancellationToken));
        return dtos;
    }

    public async Task<OperationResult> ReorderStepsAsync(Guid routeBlueprintId, IEnumerable<Guid> stepIdsInOrder, CancellationToken cancellationToken = default)
    {
        var steps = await _unitOfWork.RouteSteps.FindAsync(s => s.RouteBlueprintId == routeBlueprintId, cancellationToken);
        var stepDict = steps.ToDictionary(s => s.Id);
        
        int order = 1;
        foreach (var stepId in stepIdsInOrder)
        {
            if (stepDict.TryGetValue(stepId, out var step))
            {
                step.StepOrder = order++;
                step.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.RouteSteps.Update(step);
            }
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Pasos reordenados exitosamente");
    }

    public async Task<OperationResult<RouteStepResponse>> AddStepToRouteAsync(Guid routeBlueprintId, CreateRouteStepRequest request, CancellationToken cancellationToken = default)
    {
        // Get max step order
        var existingSteps = await _unitOfWork.RouteSteps.FindAsync(s => s.RouteBlueprintId == routeBlueprintId, cancellationToken);
        var maxOrder = existingSteps.Any() ? existingSteps.Max(s => s.StepOrder) : 0;
        
        var newRequest = new CreateRouteStepRequest(routeBlueprintId, request.LocationId, maxOrder + 1, request.StandardTransitTime, request.StepType);
        return await CreateAsync(newRequest, cancellationToken);
    }

    private async Task<RouteStepResponse> MapToResponseAsync(RouteStep s, CancellationToken ct)
    {
        var route = await _unitOfWork.RouteBlueprints.GetByIdAsync(s.RouteBlueprintId, ct);
        var location = await _unitOfWork.Locations.GetByIdAsync(s.LocationId, ct);
        return new RouteStepResponse(s.Id, s.RouteBlueprintId, route?.Name ?? "", s.LocationId, location?.Name ?? "", 
            s.StepOrder, s.StandardTransitTime, s.StepType.ToString(), s.CreatedAt, s.UpdatedAt);
    }
}
