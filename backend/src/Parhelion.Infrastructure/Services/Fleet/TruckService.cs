using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Services.Fleet;

/// <summary>
/// Implementación del servicio de Trucks.
/// </summary>
public class TruckService : ITruckService
{
    private readonly IUnitOfWork _unitOfWork;

    public TruckService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<TruckResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Trucks.GetPagedAsync(request, filter: null, orderBy: q => q.OrderBy(t => t.Plate), cancellationToken);
        return PagedResult<TruckResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<TruckResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Trucks.GetByIdAsync(id, cancellationToken);
        return entity != null ? MapToResponse(entity) : null;
    }

    public async Task<OperationResult<TruckResponse>> CreateAsync(CreateTruckRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.Trucks.FirstOrDefaultAsync(t => t.Plate == request.Plate, cancellationToken);
        if (existing != null) return OperationResult<TruckResponse>.Fail($"Ya existe un camión con placa '{request.Plate}'");

        if (!Enum.TryParse<TruckType>(request.Type, out var truckType))
            return OperationResult<TruckResponse>.Fail("Tipo de camión inválido");

        var entity = new Truck
        {
            Id = Guid.NewGuid(),
            Plate = request.Plate,
            Model = request.Model,
            Type = truckType,
            MaxCapacityKg = request.MaxCapacityKg,
            MaxVolumeM3 = request.MaxVolumeM3,
            IsActive = true,
            Vin = request.Vin,
            EngineNumber = request.EngineNumber,
            Year = request.Year,
            Color = request.Color,
            InsurancePolicy = request.InsurancePolicy,
            InsuranceExpiration = request.InsuranceExpiration,
            VerificationNumber = request.VerificationNumber,
            VerificationExpiration = request.VerificationExpiration,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Trucks.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<TruckResponse>.Ok(MapToResponse(entity), "Camión creado exitosamente");
    }

    public async Task<OperationResult<TruckResponse>> UpdateAsync(Guid id, UpdateTruckRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Trucks.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult<TruckResponse>.Fail("Camión no encontrado");

        var existingPlate = await _unitOfWork.Trucks.FirstOrDefaultAsync(t => t.Plate == request.Plate && t.Id != id, cancellationToken);
        if (existingPlate != null) return OperationResult<TruckResponse>.Fail($"La placa '{request.Plate}' ya está en uso");

        if (!Enum.TryParse<TruckType>(request.Type, out var truckType))
            return OperationResult<TruckResponse>.Fail("Tipo de camión inválido");

        entity.Plate = request.Plate;
        entity.Model = request.Model;
        entity.Type = truckType;
        entity.MaxCapacityKg = request.MaxCapacityKg;
        entity.MaxVolumeM3 = request.MaxVolumeM3;
        entity.IsActive = request.IsActive;
        entity.Vin = request.Vin;
        entity.EngineNumber = request.EngineNumber;
        entity.Year = request.Year;
        entity.Color = request.Color;
        entity.InsurancePolicy = request.InsurancePolicy;
        entity.InsuranceExpiration = request.InsuranceExpiration;
        entity.VerificationNumber = request.VerificationNumber;
        entity.VerificationExpiration = request.VerificationExpiration;
        entity.LastMaintenanceDate = request.LastMaintenanceDate;
        entity.NextMaintenanceDate = request.NextMaintenanceDate;
        entity.CurrentOdometerKm = request.CurrentOdometerKm;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Trucks.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<TruckResponse>.Ok(MapToResponse(entity), "Camión actualizado exitosamente");
    }

    public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Trucks.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult.Fail("Camión no encontrado");
        _unitOfWork.Trucks.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Camión eliminado exitosamente");
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _unitOfWork.Trucks.AnyAsync(t => t.Id == id, cancellationToken);

    public async Task<TruckResponse?> GetByPlateAsync(string plate, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Trucks.FirstOrDefaultAsync(t => t.Plate == plate, cancellationToken);
        return entity != null ? MapToResponse(entity) : null;
    }

    public async Task<PagedResult<TruckResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Trucks.GetPagedAsync(request, filter: t => t.TenantId == tenantId, orderBy: q => q.OrderBy(t => t.Plate), cancellationToken);
        return PagedResult<TruckResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<PagedResult<TruckResponse>> GetByActiveStatusAsync(Guid tenantId, bool isActive, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Trucks.GetPagedAsync(request, filter: t => t.TenantId == tenantId && t.IsActive == isActive, orderBy: q => q.OrderBy(t => t.Plate), cancellationToken);
        return PagedResult<TruckResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<PagedResult<TruckResponse>> GetByTypeAsync(Guid tenantId, TruckType truckType, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Trucks.GetPagedAsync(request, filter: t => t.TenantId == tenantId && t.Type == truckType, orderBy: q => q.OrderBy(t => t.Plate), cancellationToken);
        return PagedResult<TruckResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<OperationResult<TruckResponse>> SetActiveStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Trucks.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult<TruckResponse>.Fail("Camión no encontrado");

        entity.IsActive = isActive;
        entity.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Trucks.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<TruckResponse>.Ok(MapToResponse(entity), $"Estado actualizado");
    }

    public async Task<PagedResult<TruckResponse>> GetAvailableAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Trucks.GetPagedAsync(request, filter: t => t.TenantId == tenantId && t.IsActive, orderBy: q => q.OrderBy(t => t.Plate), cancellationToken);
        return PagedResult<TruckResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    private static TruckResponse MapToResponse(Truck e) => new(e.Id, e.Plate, e.Model, e.Type.ToString(), e.MaxCapacityKg, e.MaxVolumeM3,
        e.IsActive, e.Vin, e.EngineNumber, e.Year, e.Color, e.InsurancePolicy, e.InsuranceExpiration, e.VerificationNumber,
        e.VerificationExpiration, e.LastMaintenanceDate, e.NextMaintenanceDate, e.CurrentOdometerKm, e.CreatedAt, e.UpdatedAt);
}
