namespace Parhelion.Application.DTOs.Warehouse;

// ========== LOCATION DTOs ==========

public record CreateLocationRequest(
    string Code,
    string Name,
    string Type,
    string FullAddress,
    decimal? Latitude,
    decimal? Longitude,
    bool CanReceive,
    bool CanDispatch,
    bool IsInternal
);

public record UpdateLocationRequest(
    string Code,
    string Name,
    string Type,
    string FullAddress,
    decimal? Latitude,
    decimal? Longitude,
    bool CanReceive,
    bool CanDispatch,
    bool IsInternal,
    bool IsActive
);

public record LocationResponse(
    Guid Id,
    string Code,
    string Name,
    string Type,
    string FullAddress,
    decimal? Latitude,
    decimal? Longitude,
    bool CanReceive,
    bool CanDispatch,
    bool IsInternal,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ========== WAREHOUSE ZONE DTOs ==========

public record CreateWarehouseZoneRequest(
    Guid LocationId,
    string Code,
    string Name,
    string Type
);

public record UpdateWarehouseZoneRequest(
    string Code,
    string Name,
    string Type,
    bool IsActive
);

public record WarehouseZoneResponse(
    Guid Id,
    Guid LocationId,
    string LocationName,
    string Code,
    string Name,
    string Type,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ========== WAREHOUSE OPERATOR DTOs ==========

public record CreateWarehouseOperatorRequest(
    Guid EmployeeId,
    Guid AssignedLocationId,
    Guid? PrimaryZoneId
);

public record UpdateWarehouseOperatorRequest(
    Guid AssignedLocationId,
    Guid? PrimaryZoneId
);

public record WarehouseOperatorResponse(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid AssignedLocationId,
    string LocationName,
    Guid? PrimaryZoneId,
    string? ZoneName,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ========== INVENTORY STOCK DTOs ==========

public record CreateInventoryStockRequest(
    Guid ZoneId,
    Guid ProductId,
    decimal Quantity,
    decimal QuantityReserved,
    string? BatchNumber,
    DateTime? ExpiryDate,
    decimal? UnitCost
);

public record UpdateInventoryStockRequest(
    decimal Quantity,
    decimal QuantityReserved,
    string? BatchNumber,
    DateTime? ExpiryDate,
    DateTime? LastCountDate,
    decimal? UnitCost
);

public record InventoryStockResponse(
    Guid Id,
    Guid ZoneId,
    string ZoneName,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    decimal Quantity,
    decimal QuantityReserved,
    decimal QuantityAvailable,
    string? BatchNumber,
    DateTime? ExpiryDate,
    DateTime? LastCountDate,
    decimal? UnitCost,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ========== INVENTORY TRANSACTION DTOs ==========

public record CreateInventoryTransactionRequest(
    Guid ProductId,
    Guid? OriginZoneId,
    Guid? DestinationZoneId,
    decimal Quantity,
    string TransactionType,
    Guid? ShipmentId,
    string? BatchNumber,
    string? Remarks
);

public record InventoryTransactionResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid? OriginZoneId,
    string? OriginZoneName,
    Guid? DestinationZoneId,
    string? DestinationZoneName,
    decimal Quantity,
    string TransactionType,
    Guid PerformedByUserId,
    string PerformedByName,
    Guid? ShipmentId,
    string? BatchNumber,
    string? Remarks,
    DateTime Timestamp,
    DateTime CreatedAt
);
