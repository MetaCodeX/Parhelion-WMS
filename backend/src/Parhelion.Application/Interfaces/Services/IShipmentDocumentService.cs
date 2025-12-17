using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de ShipmentDocuments (documentos de envío).
/// Maneja documentos legales como Carta Porte, POD, Manifiestos, etc.
/// </summary>
public interface IShipmentDocumentService
{
    /// <summary>
    /// Obtiene documentos con paginación.
    /// </summary>
    Task<PagedResult<ShipmentDocumentResponse>> GetAllAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un documento por ID.
    /// </summary>
    Task<ShipmentDocumentResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los documentos de un envío.
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de documentos del envío.</returns>
    Task<IEnumerable<ShipmentDocumentResponse>> GetByShipmentAsync(
        Guid shipmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea un nuevo documento de envío.
    /// </summary>
    /// <param name="request">Datos del documento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<OperationResult<ShipmentDocumentResponse>> CreateAsync(
        CreateShipmentDocumentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene documentos por tipo.
    /// </summary>
    /// <param name="shipmentId">ID del envío.</param>
    /// <param name="documentType">Tipo de documento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de documentos del tipo especificado.</returns>
    Task<IEnumerable<ShipmentDocumentResponse>> GetByTypeAsync(
        Guid shipmentId,
        DocumentType documentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un documento.
    /// </summary>
    Task<OperationResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
