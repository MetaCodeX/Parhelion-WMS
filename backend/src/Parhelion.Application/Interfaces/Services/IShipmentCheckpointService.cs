using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de ShipmentCheckpoints (eventos de trazabilidad).
/// Maneja el registro de eventos durante el ciclo de vida del envío.
/// </summary>
public interface IShipmentCheckpointService
{
    /// <summary>
    /// Obtiene checkpoints con paginación.
    /// </summary>
    Task<PagedResult<ShipmentCheckpointResponse>> GetAllAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un checkpoint por ID.
    /// </summary>
    Task<ShipmentCheckpointResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los checkpoints de un envío ordenados por timestamp.
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de checkpoints ordenada cronológicamente.</returns>
    Task<IEnumerable<ShipmentCheckpointResponse>> GetByShipmentAsync(
        Guid shipmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea un nuevo checkpoint de trazabilidad.
    /// </summary>
    /// <param name="request">Datos del checkpoint.</param>
    /// <param name="createdByUserId">ID del usuario que crea el checkpoint.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<OperationResult<ShipmentCheckpointResponse>> CreateAsync(
        CreateShipmentCheckpointRequest request,
        Guid createdByUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene checkpoints por estatus.
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="statusCode">Código de estatus.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de checkpoints con el estatus especificado.</returns>
    Task<IEnumerable<ShipmentCheckpointResponse>> GetByStatusCodeAsync(
        Guid shipmentId,
        string statusCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el último checkpoint de un envío.
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Último checkpoint o null.</returns>
    Task<ShipmentCheckpointResponse?> GetLastCheckpointAsync(
        Guid shipmentId,
        CancellationToken cancellationToken = default);
}
