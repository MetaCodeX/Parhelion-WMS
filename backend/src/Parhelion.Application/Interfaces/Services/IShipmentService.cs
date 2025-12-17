using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de Shipments (envíos).
/// Maneja el ciclo de vida completo de un envío desde creación hasta entrega.
/// </summary>
public interface IShipmentService : IGenericService<Shipment, ShipmentResponse, CreateShipmentRequest, UpdateShipmentRequest>
{
    /// <summary>
    /// Busca un envío por su número de tracking.
    /// </summary>
    /// <param name="trackingNumber">Número de tracking (ej: PAR-123456).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Envío encontrado o null.</returns>
    Task<ShipmentResponse?> GetByTrackingNumberAsync(
        string trackingNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene envíos por tenant.
    /// </summary>
    /// <param name="tenantId">ID del tenant.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de envíos del tenant.</returns>
    Task<PagedResult<ShipmentResponse>> GetByTenantAsync(
        Guid tenantId,
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene envíos por estatus.
    /// </summary>
    /// <param name="tenantId">ID del tenant.</param>
    /// <param name="status">Estatus del envío.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de envíos con el estatus especificado.</returns>
    Task<PagedResult<ShipmentResponse>> GetByStatusAsync(
        Guid tenantId,
        ShipmentStatus status,
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene envíos asignados a un chofer.
    /// </summary>
    /// <param name="driverId">ID del chofer.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de envíos del chofer.</returns>
    Task<PagedResult<ShipmentResponse>> GetByDriverAsync(
        Guid driverId,
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene envíos por ubicación de origen o destino.
    /// </summary>
    /// <param name="locationId">ID de la ubicación.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de envíos de la ubicación.</returns>
    Task<PagedResult<ShipmentResponse>> GetByLocationAsync(
        Guid locationId,
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asigna un envío a un chofer y camión.
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="driverId">ID del chofer.</param>
    /// <param name="truckId">ID del camión.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<OperationResult<ShipmentResponse>> AssignToDriverAsync(
        Guid shipmentId,
        Guid driverId,
        Guid truckId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza el estatus de un envío.
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="newStatus">Nuevo estatus.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<OperationResult<ShipmentResponse>> UpdateStatusAsync(
        Guid shipmentId,
        ShipmentStatus newStatus,
        CancellationToken cancellationToken = default);
}
