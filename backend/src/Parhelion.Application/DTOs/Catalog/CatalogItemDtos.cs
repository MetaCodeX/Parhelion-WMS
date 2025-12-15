namespace Parhelion.Application.DTOs.Catalog;

/// <summary>
/// Request para crear un nuevo CatalogItem.
/// </summary>
public record CreateCatalogItemRequest(
    string Sku,
    string Name,
    string? Description,
    string BaseUom,
    decimal DefaultWeightKg,
    decimal DefaultWidthCm,
    decimal DefaultHeightCm,
    decimal DefaultLengthCm,
    bool RequiresRefrigeration,
    bool IsHazardous,
    bool IsFragile
);

/// <summary>
/// Request para actualizar un CatalogItem existente.
/// </summary>
public record UpdateCatalogItemRequest(
    string Name,
    string? Description,
    string BaseUom,
    decimal DefaultWeightKg,
    decimal DefaultWidthCm,
    decimal DefaultHeightCm,
    decimal DefaultLengthCm,
    bool RequiresRefrigeration,
    bool IsHazardous,
    bool IsFragile,
    bool IsActive
);

/// <summary>
/// Response DTO para CatalogItem.
/// Incluye todos los campos relevantes para el cliente.
/// </summary>
public record CatalogItemResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string BaseUom,
    decimal DefaultWeightKg,
    decimal DefaultWidthCm,
    decimal DefaultHeightCm,
    decimal DefaultLengthCm,
    decimal DefaultVolumeM3,
    bool RequiresRefrigeration,
    bool IsHazardous,
    bool IsFragile,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
