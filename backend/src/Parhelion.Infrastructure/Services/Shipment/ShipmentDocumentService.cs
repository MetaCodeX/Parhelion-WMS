using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Shipment;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Infrastructure.Services.Shipment;

/// <summary>
/// Implementación del servicio de ShipmentDocuments.
/// </summary>
public class ShipmentDocumentService : IShipmentDocumentService
{
    private readonly IUnitOfWork _unitOfWork;

    public ShipmentDocumentService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<ShipmentDocumentResponse>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.ShipmentDocuments.GetPagedAsync(request, filter: null, orderBy: q => q.OrderByDescending(d => d.GeneratedAt), cancellationToken);
        return PagedResult<ShipmentDocumentResponse>.From(items.Select(MapToResponse), totalCount, request);
    }

    public async Task<ShipmentDocumentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.ShipmentDocuments.GetByIdAsync(id, cancellationToken);
        return entity != null ? MapToResponse(entity) : null;
    }

    public async Task<IEnumerable<ShipmentDocumentResponse>> GetByShipmentAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var docs = await _unitOfWork.ShipmentDocuments.FindAsync(d => d.ShipmentId == shipmentId, cancellationToken);
        return docs.Select(MapToResponse);
    }

    public async Task<OperationResult<ShipmentDocumentResponse>> CreateAsync(CreateShipmentDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var shipment = await _unitOfWork.Shipments.GetByIdAsync(request.ShipmentId, cancellationToken);
        if (shipment == null) return OperationResult<ShipmentDocumentResponse>.Fail("Envío no encontrado");

        if (!Enum.TryParse<DocumentType>(request.DocumentType, out var docType))
            return OperationResult<ShipmentDocumentResponse>.Fail("Tipo de documento inválido");

        var entity = new ShipmentDocument
        {
            Id = Guid.NewGuid(),
            ShipmentId = request.ShipmentId,
            DocumentType = docType,
            FileUrl = request.FileUrl,
            GeneratedBy = request.GeneratedBy,
            GeneratedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ShipmentDocuments.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult<ShipmentDocumentResponse>.Ok(MapToResponse(entity), "Documento creado exitosamente");
    }

    public async Task<IEnumerable<ShipmentDocumentResponse>> GetByTypeAsync(Guid shipmentId, DocumentType documentType, CancellationToken cancellationToken = default)
    {
        var docs = await _unitOfWork.ShipmentDocuments.FindAsync(d => d.ShipmentId == shipmentId && d.DocumentType == documentType, cancellationToken);
        return docs.Select(MapToResponse);
    }

    public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.ShipmentDocuments.GetByIdAsync(id, cancellationToken);
        if (entity == null) return OperationResult.Fail("Documento no encontrado");
        _unitOfWork.ShipmentDocuments.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Documento eliminado exitosamente");
    }

    private static ShipmentDocumentResponse MapToResponse(ShipmentDocument e) => new(
        e.Id, e.ShipmentId, e.DocumentType.ToString(), e.FileUrl, e.GeneratedBy, e.GeneratedAt, e.ExpiresAt, e.CreatedAt);
}
