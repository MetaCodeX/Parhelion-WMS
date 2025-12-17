using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Services.Fleet;

/// <summary>
/// Implementación del servicio de FleetLogs.
/// </summary>
public class FleetLogService : IFleetLogService
{
    private readonly IUnitOfWork _unitOfWork;

    public FleetLogService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<FleetLogResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.FleetLogs.GetPagedAsync(request, filter: null, orderBy: q => q.OrderByDescending(l => l.Timestamp), cancellationToken);
        var dtos = new List<FleetLogResponse>();
        foreach (var l in items) dtos.Add(await MapToResponseAsync(l, cancellationToken));
        return PagedResult<FleetLogResponse>.From(dtos, totalCount, request);
    }

    public async Task<FleetLogResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.FleetLogs.GetByIdAsync(id, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    public async Task<PagedResult<FleetLogResponse>> GetByDriverAsync(Guid driverId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.FleetLogs.GetPagedAsync(request, filter: l => l.DriverId == driverId, orderBy: q => q.OrderByDescending(l => l.Timestamp), cancellationToken);
        var dtos = new List<FleetLogResponse>();
        foreach (var l in items) dtos.Add(await MapToResponseAsync(l, cancellationToken));
        return PagedResult<FleetLogResponse>.From(dtos, totalCount, request);
    }

    public async Task<PagedResult<FleetLogResponse>> GetByTruckAsync(Guid truckId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.FleetLogs.GetPagedAsync(request, filter: l => l.OldTruckId == truckId || l.NewTruckId == truckId, orderBy: q => q.OrderByDescending(l => l.Timestamp), cancellationToken);
        var dtos = new List<FleetLogResponse>();
        foreach (var l in items) dtos.Add(await MapToResponseAsync(l, cancellationToken));
        return PagedResult<FleetLogResponse>.From(dtos, totalCount, request);
    }

    public async Task<OperationResult<FleetLogResponse>> StartUsageAsync(Guid driverId, Guid truckId, CancellationToken cancellationToken = default)
    {
        var driver = await _unitOfWork.Drivers.GetByIdAsync(driverId, cancellationToken);
        if (driver == null) return OperationResult<FleetLogResponse>.Fail("Chofer no encontrado");

        var truck = await _unitOfWork.Trucks.GetByIdAsync(truckId, cancellationToken);
        if (truck == null) return OperationResult<FleetLogResponse>.Fail("Camión no encontrado");

        var entity = new FleetLog
        {
            Id = Guid.NewGuid(),
            TenantId = truck.TenantId,
            DriverId = driverId,
            OldTruckId = driver.CurrentTruckId,
            NewTruckId = truckId,
            Reason = FleetLogReason.Reassignment,
            Timestamp = DateTime.UtcNow,
            CreatedByUserId = Guid.Empty, // Should be injected from context
            CreatedAt = DateTime.UtcNow
        };

        // Update driver's current truck
        driver.CurrentTruckId = truckId;
        driver.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Drivers.Update(driver);

        await _unitOfWork.FleetLogs.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<FleetLogResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Uso de camión iniciado");
    }

    public async Task<OperationResult<FleetLogResponse>> EndUsageAsync(Guid logId, decimal? endOdometer, CancellationToken cancellationToken = default)
    {
        // FleetLog doesn't have end tracking in current model, this is for future extension
        var entity = await _unitOfWork.FleetLogs.GetByIdAsync(logId, cancellationToken);
        if (entity == null) return OperationResult<FleetLogResponse>.Fail("Log no encontrado");
        
        return OperationResult<FleetLogResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Uso de camión finalizado");
    }

    public async Task<FleetLogResponse?> GetActiveLogForDriverAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        var logs = await _unitOfWork.FleetLogs.FindAsync(l => l.DriverId == driverId, cancellationToken);
        var lastLog = logs.OrderByDescending(l => l.Timestamp).FirstOrDefault();
        return lastLog != null ? await MapToResponseAsync(lastLog, cancellationToken) : null;
    }

    private async Task<FleetLogResponse> MapToResponseAsync(FleetLog e, CancellationToken ct)
    {
        var driver = await _unitOfWork.Drivers.GetByIdAsync(e.DriverId, ct);
        string driverName = "Unknown";
        if (driver != null)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(driver.EmployeeId, ct);
            if (employee != null)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(employee.UserId, ct);
                driverName = user?.FullName ?? "Unknown";
            }
        }

        var oldTruck = e.OldTruckId.HasValue ? await _unitOfWork.Trucks.GetByIdAsync(e.OldTruckId.Value, ct) : null;
        var newTruck = await _unitOfWork.Trucks.GetByIdAsync(e.NewTruckId, ct);
        var createdBy = await _unitOfWork.Users.GetByIdAsync(e.CreatedByUserId, ct);

        return new FleetLogResponse(e.Id, e.DriverId, driverName, e.OldTruckId, oldTruck?.Plate, e.NewTruckId,
            newTruck?.Plate ?? "Unknown", e.Reason.ToString(), e.Timestamp, e.CreatedByUserId, createdBy?.FullName ?? "System", e.CreatedAt);
    }
}
