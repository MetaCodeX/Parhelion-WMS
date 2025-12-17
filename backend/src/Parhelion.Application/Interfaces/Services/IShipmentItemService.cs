using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Domain.Entities;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de ShipmentItems (partidas del manifiesto de carga).
/// Maneja items individuales dentro de un envío con sus características y restricciones.
/// </summary>
public interface IShipmentItemService : IGenericService<ShipmentItem, ShipmentItemResponse, CreateShipmentItemRequest, UpdateShipmentItemRequest>
{
    /// <summary>
    /// Obtiene todos los items de un envío.
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de items del envío.</returns>
    Task<PagedResult<ShipmentItemResponse>> GetByShipmentAsync(
        Guid shipmentId,
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula el peso volumétrico de un item.
    /// Fórmula: (Alto x Ancho x Largo) / Factor Dimensional
    /// </summary>
    /// <param name="widthCm">Ancho en centímetros.</param>
    /// <param name="heightCm">Alto en centímetros.</param>
    /// <param name="lengthCm">Largo en centímetros.</param>
    /// <param name="factorDimensional">Factor dimensional (default: 5000).</param>
    /// <returns>Peso volumétrico en kilogramos.</returns>
    decimal CalculateVolumetricWeight(
        decimal widthCm,
        decimal heightCm,
        decimal lengthCm,
        decimal factorDimensional = 5000);

    /// <summary>
    /// Obtiene items que requieren refrigeración de un envío.
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de items con RequiresRefrigeration = true.</returns>
    Task<IEnumerable<ShipmentItemResponse>> GetRefrigeratedItemsAsync(
        Guid shipmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene items peligrosos (HAZMAT) de un envío.
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de items con IsHazardous = true.</returns>
    Task<IEnumerable<ShipmentItemResponse>> GetHazardousItemsAsync(
        Guid shipmentId,
        CancellationToken cancellationToken = default);
}
