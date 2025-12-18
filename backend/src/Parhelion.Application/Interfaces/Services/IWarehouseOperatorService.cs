using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Warehouse;
using Parhelion.Domain.Entities;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de operadores de almacén.
/// </summary>
public interface IWarehouseOperatorService : IGenericService<WarehouseOperator, WarehouseOperatorResponse, CreateWarehouseOperatorRequest, UpdateWarehouseOperatorRequest>
{
    /// <summary>
    /// Obtiene operadores por ubicación asignada.
    /// </summary>
    Task<PagedResult<WarehouseOperatorResponse>> GetByLocationAsync(Guid locationId, PagedRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene operador por empleado.
    /// </summary>
    Task<WarehouseOperatorResponse?> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asigna operador a una zona.
    /// </summary>
    Task<OperationResult<WarehouseOperatorResponse>> AssignToZoneAsync(Guid operatorId, Guid zoneId, CancellationToken cancellationToken = default);
}
