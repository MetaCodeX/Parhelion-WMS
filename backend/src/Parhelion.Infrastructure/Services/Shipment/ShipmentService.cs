using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Services.Shipment;

/// <summary>
/// Implementación del servicio de Shipments.
/// Gestiona el ciclo de vida de envíos desde creación hasta entrega.
/// </summary>
public class ShipmentService : IShipmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public ShipmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ShipmentResponse>> GetAllAsync(
        PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Shipments.GetPagedAsync(
            request, filter: null, orderBy: q => q.OrderByDescending(s => s.CreatedAt), cancellationToken);
        var dtos = new List<ShipmentResponse>();
        foreach (var s in items) dtos.Add(await MapToResponseAsync(s, cancellationToken));
        return PagedResult<ShipmentResponse>.From(dtos, totalCount, request);
    }

    public async Task<ShipmentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Shipments.GetByIdAsync(id, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    public async Task<OperationResult<ShipmentResponse>> CreateAsync(
        CreateShipmentRequest request, CancellationToken cancellationToken = default)
    {
        var trackingNumber = GenerateTrackingNumber();
        var entity = new Domain.Entities.Shipment
        {
            Id = Guid.NewGuid(),
            TrackingNumber = trackingNumber,
            QrCodeData = $"PAR:{trackingNumber}",
            OriginLocationId = request.OriginLocationId,
            DestinationLocationId = request.DestinationLocationId,
            SenderId = request.SenderId,
            RecipientClientId = request.RecipientClientId,
            RecipientName = request.RecipientName,
            RecipientPhone = request.RecipientPhone,
            TotalWeightKg = request.TotalWeightKg,
            TotalVolumeM3 = request.TotalVolumeM3,
            DeclaredValue = request.DeclaredValue,
            SatMerchandiseCode = request.SatMerchandiseCode,
            DeliveryInstructions = request.DeliveryInstructions,
            Priority = Enum.TryParse<ShipmentPriority>(request.Priority, out var p) ? p : ShipmentPriority.Normal,
            Status = ShipmentStatus.PendingApproval,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Shipments.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<ShipmentResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Envío creado exitosamente");
    }

    public async Task<OperationResult<ShipmentResponse>> UpdateAsync(
        Guid id, UpdateShipmentRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Shipments.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult<ShipmentResponse>.Fail("Envío no encontrado");

        entity.AssignedRouteId = request.AssignedRouteId;
        entity.CurrentStepOrder = request.CurrentStepOrder;
        entity.DeliveryInstructions = request.DeliveryInstructions;
        if (Enum.TryParse<ShipmentPriority>(request.Priority, out var p)) entity.Priority = p;
        if (Enum.TryParse<ShipmentStatus>(request.Status, out var s)) entity.Status = s;
        entity.TruckId = request.TruckId;
        entity.DriverId = request.DriverId;
        entity.WasQrScanned = request.WasQrScanned;
        entity.IsDelayed = request.IsDelayed;
        entity.ScheduledDeparture = request.ScheduledDeparture;
        entity.PickupWindowStart = request.PickupWindowStart;
        entity.PickupWindowEnd = request.PickupWindowEnd;
        entity.EstimatedArrival = request.EstimatedArrival;
        entity.AssignedAt = request.AssignedAt;
        entity.DeliveredAt = request.DeliveredAt;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Shipments.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<ShipmentResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Envío actualizado exitosamente");
    }

    public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Shipments.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult.Fail("Envío no encontrado");
        _unitOfWork.Shipments.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Envío eliminado exitosamente");
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _unitOfWork.Shipments.AnyAsync(s => s.Id == id, cancellationToken);

    public async Task<ShipmentResponse?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Shipments.FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    public async Task<PagedResult<ShipmentResponse>> GetByTenantAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Shipments.GetPagedAsync(request, filter: s => s.TenantId == tenantId, orderBy: q => q.OrderByDescending(s => s.CreatedAt), cancellationToken);
        var dtos = new List<ShipmentResponse>();
        foreach (var s in items) dtos.Add(await MapToResponseAsync(s, cancellationToken));
        return PagedResult<ShipmentResponse>.From(dtos, totalCount, request);
    }

    public async Task<PagedResult<ShipmentResponse>> GetByStatusAsync(Guid tenantId, ShipmentStatus status, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Shipments.GetPagedAsync(request, filter: s => s.TenantId == tenantId && s.Status == status, orderBy: q => q.OrderByDescending(s => s.CreatedAt), cancellationToken);
        var dtos = new List<ShipmentResponse>();
        foreach (var s in items) dtos.Add(await MapToResponseAsync(s, cancellationToken));
        return PagedResult<ShipmentResponse>.From(dtos, totalCount, request);
    }

    public async Task<PagedResult<ShipmentResponse>> GetByDriverAsync(Guid driverId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Shipments.GetPagedAsync(request, filter: s => s.DriverId == driverId, orderBy: q => q.OrderByDescending(s => s.CreatedAt), cancellationToken);
        var dtos = new List<ShipmentResponse>();
        foreach (var s in items) dtos.Add(await MapToResponseAsync(s, cancellationToken));
        return PagedResult<ShipmentResponse>.From(dtos, totalCount, request);
    }

    public async Task<PagedResult<ShipmentResponse>> GetByLocationAsync(Guid locationId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Shipments.GetPagedAsync(request, filter: s => s.OriginLocationId == locationId || s.DestinationLocationId == locationId, orderBy: q => q.OrderByDescending(s => s.CreatedAt), cancellationToken);
        var dtos = new List<ShipmentResponse>();
        foreach (var s in items) dtos.Add(await MapToResponseAsync(s, cancellationToken));
        return PagedResult<ShipmentResponse>.From(dtos, totalCount, request);
    }

    public async Task<OperationResult<ShipmentResponse>> AssignToDriverAsync(Guid shipmentId, Guid driverId, Guid truckId, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Shipments.GetByIdAsync(shipmentId, cancellationToken);
        if (entity == null) return OperationResult<ShipmentResponse>.Fail("Envío no encontrado");

        var driver = await _unitOfWork.Drivers.GetByIdAsync(driverId, cancellationToken);
        if (driver == null || driver.Status != DriverStatus.Available)
            return OperationResult<ShipmentResponse>.Fail("Chofer no encontrado o no disponible");

        var truck = await _unitOfWork.Trucks.GetByIdAsync(truckId, cancellationToken);
        if (truck == null) return OperationResult<ShipmentResponse>.Fail("Camión no encontrado");

        entity.DriverId = driverId;
        entity.TruckId = truckId;
        entity.AssignedAt = DateTime.UtcNow;
        entity.Status = ShipmentStatus.Approved;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Shipments.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<ShipmentResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Envío asignado exitosamente");
    }

    public async Task<OperationResult<ShipmentResponse>> UpdateStatusAsync(Guid shipmentId, ShipmentStatus newStatus, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Shipments.GetByIdAsync(shipmentId, cancellationToken);
        if (entity == null) return OperationResult<ShipmentResponse>.Fail("Envío no encontrado");

        // Validate status transition
        var validationResult = ValidateStatusTransition(entity.Status, newStatus);
        if (!validationResult.IsValid)
            return OperationResult<ShipmentResponse>.Fail(validationResult.ErrorMessage!);

        entity.Status = newStatus;
        entity.UpdatedAt = DateTime.UtcNow;
        
        // Set DeliveredAt when status changes to Delivered
        if (newStatus == ShipmentStatus.Delivered) 
            entity.DeliveredAt = DateTime.UtcNow;

        _unitOfWork.Shipments.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<ShipmentResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), $"Estado actualizado a {newStatus}");
    }

    /// <summary>
    /// Validates if a status transition is allowed based on business rules.
    /// Valid transitions:
    /// PendingApproval → Approved, Exception
    /// Approved → Loaded, Exception
    /// Loaded → InTransit, Exception
    /// InTransit → AtHub, OutForDelivery, Exception
    /// AtHub → InTransit, OutForDelivery, Exception
    /// OutForDelivery → Delivered, Exception
    /// Exception → Any previous state (recovery)
    /// </summary>
    private static (bool IsValid, string? ErrorMessage) ValidateStatusTransition(ShipmentStatus current, ShipmentStatus next)
    {
        if (current == next)
            return (true, null); // No change is always valid

        // From Exception, can go to any state (recovery)
        if (current == ShipmentStatus.Exception)
            return (true, null);

        // Cannot go backwards in workflow (except from Exception)
        var validTransitions = current switch
        {
            ShipmentStatus.PendingApproval => new[] { ShipmentStatus.Approved, ShipmentStatus.Exception },
            ShipmentStatus.Approved => new[] { ShipmentStatus.Loaded, ShipmentStatus.Exception },
            ShipmentStatus.Loaded => new[] { ShipmentStatus.InTransit, ShipmentStatus.Exception },
            ShipmentStatus.InTransit => new[] { ShipmentStatus.AtHub, ShipmentStatus.OutForDelivery, ShipmentStatus.Exception },
            ShipmentStatus.AtHub => new[] { ShipmentStatus.InTransit, ShipmentStatus.OutForDelivery, ShipmentStatus.Exception },
            ShipmentStatus.OutForDelivery => new[] { ShipmentStatus.Delivered, ShipmentStatus.AtHub, ShipmentStatus.Exception },
            ShipmentStatus.Delivered => new[] { ShipmentStatus.Exception }, // Delivered is final, only Exception allowed
            _ => Array.Empty<ShipmentStatus>()
        };

        if (!validTransitions.Contains(next))
            return (false, $"Transición de estado inválida: {current} → {next}. Estados válidos: {string.Join(", ", validTransitions)}");

        return (true, null);
    }

    private static string GenerateTrackingNumber() => $"PAR-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";

    private async Task<ShipmentResponse> MapToResponseAsync(Domain.Entities.Shipment e, CancellationToken ct)
    {
        var origin = await _unitOfWork.Locations.GetByIdAsync(e.OriginLocationId, ct);
        var dest = await _unitOfWork.Locations.GetByIdAsync(e.DestinationLocationId, ct);
        var sender = e.SenderId.HasValue ? await _unitOfWork.Clients.GetByIdAsync(e.SenderId.Value, ct) : null;
        var recipient = e.RecipientClientId.HasValue ? await _unitOfWork.Clients.GetByIdAsync(e.RecipientClientId.Value, ct) : null;
        var truck = e.TruckId.HasValue ? await _unitOfWork.Trucks.GetByIdAsync(e.TruckId.Value, ct) : null;
        
        string? driverName = null;
        if (e.DriverId.HasValue)
        {
            var driver = await _unitOfWork.Drivers.GetByIdAsync(e.DriverId.Value, ct);
            if (driver != null)
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(driver.EmployeeId, ct);
                if (employee != null)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(employee.UserId, ct);
                    driverName = user?.FullName;
                }
            }
        }

        return new ShipmentResponse(e.Id, e.TrackingNumber, e.QrCodeData ?? "", e.OriginLocationId, origin?.Name ?? "Unknown",
            e.DestinationLocationId, dest?.Name ?? "Unknown", e.SenderId, sender?.CompanyName, e.RecipientClientId, recipient?.CompanyName,
            e.RecipientName, e.RecipientPhone, e.TotalWeightKg, e.TotalVolumeM3, e.DeclaredValue, e.SatMerchandiseCode, e.DeliveryInstructions,
            e.Priority.ToString(), e.Status.ToString(), e.TruckId, truck?.Plate, e.DriverId, driverName, e.WasQrScanned, e.IsDelayed,
            e.ScheduledDeparture, e.EstimatedArrival, e.DeliveredAt, e.CreatedAt, e.UpdatedAt);
    }
}
