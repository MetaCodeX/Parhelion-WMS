using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Services.Shipment;

/// <summary>
/// Implementación del servicio de ShipmentCheckpoints.
/// </summary>
public class ShipmentCheckpointService : IShipmentCheckpointService
{
    private readonly IUnitOfWork _unitOfWork;

    public ShipmentCheckpointService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<ShipmentCheckpointResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.ShipmentCheckpoints.GetPagedAsync(request, filter: null, orderBy: q => q.OrderByDescending(c => c.Timestamp), cancellationToken);
        var dtos = new List<ShipmentCheckpointResponse>();
        foreach (var c in items) dtos.Add(await MapToResponseAsync(c, cancellationToken));
        return PagedResult<ShipmentCheckpointResponse>.From(dtos, totalCount, request);
    }

    public async Task<ShipmentCheckpointResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.ShipmentCheckpoints.GetByIdAsync(id, cancellationToken);
        return entity != null ? await MapToResponseAsync(entity, cancellationToken) : null;
    }

    public async Task<IEnumerable<ShipmentCheckpointResponse>> GetByShipmentAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var checkpoints = await _unitOfWork.ShipmentCheckpoints.FindAsync(c => c.ShipmentId == shipmentId, cancellationToken);
        var ordered = checkpoints.OrderBy(c => c.Timestamp).ToList();
        var dtos = new List<ShipmentCheckpointResponse>();
        foreach (var c in ordered) dtos.Add(await MapToResponseAsync(c, cancellationToken));
        return dtos;
    }

    public async Task<OperationResult<ShipmentCheckpointResponse>> CreateAsync(CreateShipmentCheckpointRequest request, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        var shipment = await _unitOfWork.Shipments.GetByIdAsync(request.ShipmentId, cancellationToken);
        if (shipment == null) return OperationResult<ShipmentCheckpointResponse>.Fail("Envío no encontrado");

        if (!Enum.TryParse<CheckpointStatus>(request.StatusCode, out var statusCode))
            return OperationResult<ShipmentCheckpointResponse>.Fail("Código de estatus inválido");

        var entity = new ShipmentCheckpoint
        {
            Id = Guid.NewGuid(),
            ShipmentId = request.ShipmentId,
            LocationId = request.LocationId,
            StatusCode = statusCode,
            Remarks = request.Remarks,
            Timestamp = DateTime.UtcNow,
            CreatedByUserId = createdByUserId,
            HandledByDriverId = request.HandledByDriverId,
            LoadedOntoTruckId = request.LoadedOntoTruckId,
            ActionType = request.ActionType,
            PreviousCustodian = request.PreviousCustodian,
            NewCustodian = request.NewCustodian,
            HandledByWarehouseOperatorId = request.HandledByWarehouseOperatorId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ShipmentCheckpoints.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<ShipmentCheckpointResponse>.Ok(await MapToResponseAsync(entity, cancellationToken), "Checkpoint creado exitosamente");
    }

    public async Task<IEnumerable<ShipmentCheckpointResponse>> GetByStatusCodeAsync(Guid shipmentId, string statusCode, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<CheckpointStatus>(statusCode, out var status)) return Enumerable.Empty<ShipmentCheckpointResponse>();
        var checkpoints = await _unitOfWork.ShipmentCheckpoints.FindAsync(c => c.ShipmentId == shipmentId && c.StatusCode == status, cancellationToken);
        var dtos = new List<ShipmentCheckpointResponse>();
        foreach (var c in checkpoints) dtos.Add(await MapToResponseAsync(c, cancellationToken));
        return dtos;
    }

    public async Task<ShipmentCheckpointResponse?> GetLastCheckpointAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var checkpoints = await _unitOfWork.ShipmentCheckpoints.FindAsync(c => c.ShipmentId == shipmentId, cancellationToken);
        var last = checkpoints.OrderByDescending(c => c.Timestamp).FirstOrDefault();
        return last != null ? await MapToResponseAsync(last, cancellationToken) : null;
    }

    private async Task<ShipmentCheckpointResponse> MapToResponseAsync(ShipmentCheckpoint e, CancellationToken ct)
    {
        var location = e.LocationId.HasValue ? await _unitOfWork.Locations.GetByIdAsync(e.LocationId.Value, ct) : null;
        var createdBy = await _unitOfWork.Users.GetByIdAsync(e.CreatedByUserId, ct);

        string? driverName = null;
        if (e.HandledByDriverId.HasValue)
        {
            var driver = await _unitOfWork.Drivers.GetByIdAsync(e.HandledByDriverId.Value, ct);
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

        var truck = e.LoadedOntoTruckId.HasValue ? await _unitOfWork.Trucks.GetByIdAsync(e.LoadedOntoTruckId.Value, ct) : null;

        return new ShipmentCheckpointResponse(e.Id, e.ShipmentId, e.LocationId, location?.Name, e.StatusCode.ToString(), e.Remarks,
            e.Timestamp, e.CreatedByUserId, createdBy?.FullName ?? "Unknown", e.HandledByDriverId, driverName, e.LoadedOntoTruckId,
            truck?.Plate, e.ActionType, e.PreviousCustodian, e.NewCustodian, e.Latitude, e.Longitude, e.CreatedAt);
    }
}
