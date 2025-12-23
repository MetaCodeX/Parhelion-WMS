using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Fleet;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Services.Fleet;

/// <summary>
/// Implementación del servicio de Drivers.
/// </summary>
public class DriverService : IDriverService
{
    private readonly IUnitOfWork _unitOfWork;

    public DriverService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<DriverResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Drivers.GetPagedAsync(request, filter: null, orderBy: q => q.OrderBy(d => d.CreatedAt), cancellationToken);
        var dtos = new List<DriverResponse>();
        foreach (var d in items) dtos.Add(await MapToResponseAsync(d, cancellationToken));
        return PagedResult<DriverResponse>.From(dtos, totalCount, request);
    }

    public async Task<DriverResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Drivers.GetByIdAsync(id, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    public async Task<OperationResult<DriverResponse>> CreateAsync(CreateDriverRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null) return OperationResult<DriverResponse>.Fail("Empleado no encontrado");

        var existing = await _unitOfWork.Drivers.FirstOrDefaultAsync(d => d.EmployeeId == request.EmployeeId, cancellationToken);
        if (existing != null) return OperationResult<DriverResponse>.Fail("Este empleado ya tiene registro de chofer");

        if (!Enum.TryParse<DriverStatus>(request.Status, out var status)) status = DriverStatus.Available;

        var entity = new Driver
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            LicenseNumber = request.LicenseNumber,
            LicenseType = request.LicenseType,
            LicenseExpiration = request.LicenseExpiration,
            DefaultTruckId = request.DefaultTruckId,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Drivers.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<DriverResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Chofer creado exitosamente");
    }

    public async Task<OperationResult<DriverResponse>> UpdateAsync(Guid id, UpdateDriverRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Drivers.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult<DriverResponse>.Fail("Chofer no encontrado");

        entity.LicenseNumber = request.LicenseNumber;
        entity.LicenseType = request.LicenseType;
        entity.LicenseExpiration = request.LicenseExpiration;
        entity.DefaultTruckId = request.DefaultTruckId;
        entity.CurrentTruckId = request.CurrentTruckId;
        if (Enum.TryParse<DriverStatus>(request.Status, out var status)) entity.Status = status;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Drivers.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<DriverResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Chofer actualizado exitosamente");
    }

    public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Drivers.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult.Fail("Chofer no encontrado");
        _unitOfWork.Drivers.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Chofer eliminado exitosamente");
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _unitOfWork.Drivers.AnyAsync(d => d.Id == id, cancellationToken);

    public async Task<DriverResponse?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Drivers.FirstOrDefaultAsync(d => d.EmployeeId == employeeId, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    public async Task<PagedResult<DriverResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        // Driver doesn't have TenantId directly, must filter via Employee
        var employees = await _unitOfWork.Employees.FindAsync(e => e.TenantId == tenantId, cancellationToken);
        var employeeIds = employees.Select(e => e.Id).ToHashSet();
        
        var (items, totalCount) = await _unitOfWork.Drivers.GetPagedAsync(request, 
            filter: d => employeeIds.Contains(d.EmployeeId), 
            orderBy: q => q.OrderBy(d => d.CreatedAt), cancellationToken);
        
        var dtos = new List<DriverResponse>();
        foreach (var d in items) dtos.Add(await MapToResponseAsync(d, cancellationToken));
        return PagedResult<DriverResponse>.From(dtos, totalCount, request);
    }

    public async Task<PagedResult<DriverResponse>> GetByStatusAsync(Guid tenantId, DriverStatus status, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var employees = await _unitOfWork.Employees.FindAsync(e => e.TenantId == tenantId, cancellationToken);
        var employeeIds = employees.Select(e => e.Id).ToHashSet();
        
        var (items, totalCount) = await _unitOfWork.Drivers.GetPagedAsync(request, 
            filter: d => employeeIds.Contains(d.EmployeeId) && d.Status == status, 
            orderBy: q => q.OrderBy(d => d.CreatedAt), cancellationToken);
        
        var dtos = new List<DriverResponse>();
        foreach (var d in items) dtos.Add(await MapToResponseAsync(d, cancellationToken));
        return PagedResult<DriverResponse>.From(dtos, totalCount, request);
    }

    public async Task<PagedResult<DriverResponse>> GetAvailableAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default) =>
        await GetByStatusAsync(tenantId, DriverStatus.Available, request, cancellationToken);

    public async Task<OperationResult<DriverResponse>> UpdateStatusAsync(Guid id, DriverStatus status, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Drivers.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult<DriverResponse>.Fail("Chofer no encontrado");

        entity.Status = status;
        entity.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Drivers.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<DriverResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), $"Estado actualizado a {status}");
    }

    public async Task<PagedResult<DriverResponse>> GetNearbyDriversAsync(decimal lat, decimal lon, double radiusKm, Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Obtener empleados del tenant
        var employees = await _unitOfWork.Employees.FindAsync(e => e.TenantId == tenantId, cancellationToken);
        var employeeIds = employees.Select(e => e.Id).ToHashSet();

        // 2. Obtener choferes activos con camión asignado
        // Nota: Idealmente esto se haría con PostGIS, pero para MVP filtramos en memoria
        var drivers = await _unitOfWork.Drivers.FindAsync(
            d => employeeIds.Contains(d.EmployeeId) && 
                 d.Status == DriverStatus.Available && 
                 d.CurrentTruckId != null, 
            cancellationToken);

        var resultDtos = new List<DriverResponse>();

        foreach (var driver in drivers)
        {
            var truck = await _unitOfWork.Trucks.GetByIdAsync(driver.CurrentTruckId!.Value, cancellationToken);
            
            // Si el camión no tiene ubicación, saltar
            if (truck == null || truck.LastLatitude == null || truck.LastLongitude == null) continue;

            // 3. Fórmula del Haversine
            var distance = CalculateDistance(
                (double)lat, (double)lon, 
                (double)truck.LastLatitude.Value, (double)truck.LastLongitude.Value);

            if (distance <= radiusKm)
            {
                resultDtos.Add(await MapToResponseAsync(driver, cancellationToken));
            }
        }

        // Paginación en memoria
        var totalCount = resultDtos.Count;
        var pagedItems = resultDtos
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return PagedResult<DriverResponse>.From(pagedItems, totalCount, request);
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371; // Radio de la Tierra en km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double angle) => Math.PI * angle / 180.0;

    public async Task<OperationResult<DriverResponse>> AssignTruckAsync(Guid driverId, Guid truckId, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Drivers.GetByIdAsync(driverId, cancellationToken);
        if (entity == null) return OperationResult<DriverResponse>.Fail("Chofer no encontrado");

        var truck = await _unitOfWork.Trucks.GetByIdAsync(truckId, cancellationToken);
        if (truck == null) return OperationResult<DriverResponse>.Fail("Camión no encontrado");

        // Store old truck for FleetLog
        var oldTruckId = entity.CurrentTruckId;

        // Update driver's current truck
        entity.CurrentTruckId = truckId;
        entity.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Drivers.Update(entity);

        // Auto-generate FleetLog for audit trail
        var fleetLog = new FleetLog
        {
            Id = Guid.NewGuid(),
            TenantId = truck.TenantId,
            DriverId = driverId,
            OldTruckId = oldTruckId,
            NewTruckId = truckId,
            Reason = FleetLogReason.Reassignment,
            Timestamp = DateTime.UtcNow,
            CreatedByUserId = Guid.Empty, // TODO: Inject from CurrentUserService
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.FleetLogs.AddAsync(fleetLog, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<DriverResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Camión asignado exitosamente (FleetLog generado)");
    }

    private async Task<DriverResponse> MapToResponseAsync(Driver e, CancellationToken ct)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(e.EmployeeId, ct);
        var user = employee != null ? await _unitOfWork.Users.GetByIdAsync(employee.UserId, ct) : null;
        var defaultTruck = e.DefaultTruckId.HasValue ? await _unitOfWork.Trucks.GetByIdAsync(e.DefaultTruckId.Value, ct) : null;
        var currentTruck = e.CurrentTruckId.HasValue ? await _unitOfWork.Trucks.GetByIdAsync(e.CurrentTruckId.Value, ct) : null;

        return new DriverResponse(e.Id, e.EmployeeId, user?.FullName ?? "Unknown", e.LicenseNumber, e.LicenseType,
            e.LicenseExpiration, e.DefaultTruckId, defaultTruck?.Plate, e.CurrentTruckId, currentTruck?.Plate,
            e.Status.ToString(), e.CreatedAt, e.UpdatedAt);
    }
}
