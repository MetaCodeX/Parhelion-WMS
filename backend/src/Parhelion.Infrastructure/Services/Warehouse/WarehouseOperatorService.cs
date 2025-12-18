using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Services.Warehouse;

/// <summary>
/// Implementación del servicio de operadores de almacén.
/// </summary>
public class WarehouseOperatorService : IWarehouseOperatorService
{
    private readonly IUnitOfWork _unitOfWork;

    public WarehouseOperatorService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<WarehouseOperatorResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.WarehouseOperators.GetPagedAsync(request, filter: null, orderBy: q => q.OrderBy(o => o.CreatedAt), cancellationToken);
        var dtos = new List<WarehouseOperatorResponse>();
        foreach (var o in items) dtos.Add(await MapToResponseAsync(o, cancellationToken));
        return PagedResult<WarehouseOperatorResponse>.From(dtos, totalCount, request);
    }

    public async Task<WarehouseOperatorResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.WarehouseOperators.GetByIdAsync(id, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    public async Task<OperationResult<WarehouseOperatorResponse>> CreateAsync(CreateWarehouseOperatorRequest request, CancellationToken cancellationToken = default)
    {
        // Check if employee already has operator role
        var existing = await _unitOfWork.WarehouseOperators.FirstOrDefaultAsync(o => o.EmployeeId == request.EmployeeId, cancellationToken);
        if (existing != null) return OperationResult<WarehouseOperatorResponse>.Fail("El empleado ya es operador de almacén");

        var entity = new WarehouseOperator
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            AssignedLocationId = request.AssignedLocationId,
            PrimaryZoneId = request.PrimaryZoneId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.WarehouseOperators.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<WarehouseOperatorResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Operador creado exitosamente");
    }

    public async Task<OperationResult<WarehouseOperatorResponse>> UpdateAsync(Guid id, UpdateWarehouseOperatorRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.WarehouseOperators.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult<WarehouseOperatorResponse>.Fail("Operador no encontrado");

        entity.AssignedLocationId = request.AssignedLocationId;
        entity.PrimaryZoneId = request.PrimaryZoneId;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.WarehouseOperators.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<WarehouseOperatorResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Operador actualizado exitosamente");
    }

    public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.WarehouseOperators.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult.Fail("Operador no encontrado");
        _unitOfWork.WarehouseOperators.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Operador eliminado exitosamente");
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _unitOfWork.WarehouseOperators.AnyAsync(o => o.Id == id, cancellationToken);

    public async Task<PagedResult<WarehouseOperatorResponse>> GetByLocationAsync(Guid locationId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.WarehouseOperators.GetPagedAsync(request, filter: o => o.AssignedLocationId == locationId, orderBy: q => q.OrderBy(o => o.CreatedAt), cancellationToken);
        var dtos = new List<WarehouseOperatorResponse>();
        foreach (var o in items) dtos.Add(await MapToResponseAsync(o, cancellationToken));
        return PagedResult<WarehouseOperatorResponse>.From(dtos, totalCount, request);
    }

    public async Task<WarehouseOperatorResponse?> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.WarehouseOperators.FirstOrDefaultAsync(o => o.EmployeeId == employeeId, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    public async Task<OperationResult<WarehouseOperatorResponse>> AssignToZoneAsync(Guid operatorId, Guid zoneId, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.WarehouseOperators.GetByIdAsync(operatorId, cancellationToken);
        if (entity == null) return OperationResult<WarehouseOperatorResponse>.Fail("Operador no encontrado");

        var zone = await _unitOfWork.WarehouseZones.GetByIdAsync(zoneId, cancellationToken);
        if (zone == null) return OperationResult<WarehouseOperatorResponse>.Fail("Zona no encontrada");

        entity.PrimaryZoneId = zoneId;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.WarehouseOperators.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<WarehouseOperatorResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Operador asignado a zona");
    }

    private async Task<WarehouseOperatorResponse> MapToResponseAsync(WarehouseOperator o, CancellationToken ct)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(o.EmployeeId, ct);
        var user = employee != null ? await _unitOfWork.Users.GetByIdAsync(employee.UserId, ct) : null;
        var location = await _unitOfWork.Locations.GetByIdAsync(o.AssignedLocationId, ct);
        var zone = o.PrimaryZoneId.HasValue ? await _unitOfWork.WarehouseZones.GetByIdAsync(o.PrimaryZoneId.Value, ct) : null;
        
        return new WarehouseOperatorResponse(o.Id, o.EmployeeId, user?.FullName ?? "", o.AssignedLocationId, location?.Name ?? "", o.PrimaryZoneId, zone?.Name, o.CreatedAt, o.UpdatedAt);
    }
}
