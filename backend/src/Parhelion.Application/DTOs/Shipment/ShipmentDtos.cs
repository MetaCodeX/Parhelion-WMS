namespace Parhelion.Application.DTOs.Shipment;

// ========== SHIPMENT DTOs ==========

public record CreateShipmentRequest(
    Guid OriginLocationId,
    Guid DestinationLocationId,
    Guid? SenderId,
    Guid? RecipientClientId,
    string RecipientName,
    string? RecipientPhone,
    decimal TotalWeightKg,
    decimal TotalVolumeM3,
    decimal? DeclaredValue,
    string? SatMerchandiseCode,
    string? DeliveryInstructions,
    string Priority
);

public record UpdateShipmentRequest(
    Guid? AssignedRouteId,
    int? CurrentStepOrder,
    string? DeliveryInstructions,
    string Priority,
    string Status,
    Guid? TruckId,
    Guid? DriverId,
    bool WasQrScanned,
    bool IsDelayed,
    DateTime? ScheduledDeparture,
    DateTime? PickupWindowStart,
    DateTime? PickupWindowEnd,
    DateTime? EstimatedArrival,
    DateTime? AssignedAt,
    DateTime? DeliveredAt
);

public record ShipmentResponse(
    Guid Id,
    string TrackingNumber,
    string QrCodeData,
    Guid OriginLocationId,
    string OriginLocationName,
    Guid DestinationLocationId,
    string DestinationLocationName,
    Guid? SenderId,
    string? SenderName,
    Guid? RecipientClientId,
    string? RecipientClientName,
    string RecipientName,
    string? RecipientPhone,
    decimal TotalWeightKg,
    decimal TotalVolumeM3,
    decimal? DeclaredValue,
    string? SatMerchandiseCode,
    string? DeliveryInstructions,
    string Priority,
    string Status,
    Guid? TruckId,
    string? TruckPlate,
    Guid? DriverId,
    string? DriverName,
    bool WasQrScanned,
    bool IsDelayed,
    DateTime? ScheduledDeparture,
    DateTime? EstimatedArrival,
    DateTime? DeliveredAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ========== SHIPMENT ITEM DTOs ==========

public record CreateShipmentItemRequest(
    Guid ShipmentId,
    Guid? ProductId,
    string? Sku,
    string Description,
    string PackagingType,
    int Quantity,
    decimal WeightKg,
    decimal WidthCm,
    decimal HeightCm,
    decimal LengthCm,
    decimal DeclaredValue,
    bool IsFragile,
    bool IsHazardous,
    bool RequiresRefrigeration,
    string? StackingInstructions
);

public record UpdateShipmentItemRequest(
    string? Sku,
    string Description,
    string PackagingType,
    int Quantity,
    decimal WeightKg,
    decimal WidthCm,
    decimal HeightCm,
    decimal LengthCm,
    decimal DeclaredValue,
    bool IsFragile,
    bool IsHazardous,
    bool RequiresRefrigeration,
    string? StackingInstructions
);

public record ShipmentItemResponse(
    Guid Id,
    Guid ShipmentId,
    Guid? ProductId,
    string? ProductName,
    string? Sku,
    string Description,
    string PackagingType,
    int Quantity,
    decimal WeightKg,
    decimal WidthCm,
    decimal HeightCm,
    decimal LengthCm,
    decimal VolumeM3,
    decimal VolumetricWeightKg,
    decimal DeclaredValue,
    bool IsFragile,
    bool IsHazardous,
    bool RequiresRefrigeration,
    string? StackingInstructions,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ========== SHIPMENT CHECKPOINT DTOs ==========

public record CreateShipmentCheckpointRequest(
    Guid ShipmentId,
    Guid? LocationId,
    string StatusCode,
    string? Remarks,
    Guid? HandledByDriverId,
    Guid? LoadedOntoTruckId,
    string? ActionType,
    string? PreviousCustodian,
    string? NewCustodian,
    Guid? HandledByWarehouseOperatorId,
    decimal? Latitude,
    decimal? Longitude
);

public record ShipmentCheckpointResponse(
    Guid Id,
    Guid ShipmentId,
    Guid? LocationId,
    string? LocationName,
    string StatusCode,
    string? Remarks,
    DateTime Timestamp,
    Guid CreatedByUserId,
    string CreatedByName,
    Guid? HandledByDriverId,
    string? DriverName,
    Guid? LoadedOntoTruckId,
    string? TruckPlate,
    string? ActionType,
    string? PreviousCustodian,
    string? NewCustodian,
    decimal? Latitude,
    decimal? Longitude,
    DateTime CreatedAt
);

// ========== SHIPMENT DOCUMENT DTOs ==========

public record CreateShipmentDocumentRequest(
    Guid ShipmentId,
    string DocumentType,
    string FileUrl,
    string GeneratedBy,
    DateTime? ExpiresAt
);

public record ShipmentDocumentResponse(
    Guid Id,
    Guid ShipmentId,
    string DocumentType,
    string FileUrl,
    string GeneratedBy,
    DateTime GeneratedAt,
    DateTime? ExpiresAt,
    DateTime CreatedAt
);
