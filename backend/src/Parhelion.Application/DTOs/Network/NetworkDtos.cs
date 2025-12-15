namespace Parhelion.Application.DTOs.Network;

// ========== NETWORK LINK DTOs ==========

public record CreateNetworkLinkRequest(
    Guid OriginLocationId,
    Guid DestinationLocationId,
    string LinkType,
    TimeSpan TransitTime,
    bool IsBidirectional
);

public record UpdateNetworkLinkRequest(
    string LinkType,
    TimeSpan TransitTime,
    bool IsBidirectional,
    bool IsActive
);

public record NetworkLinkResponse(
    Guid Id,
    Guid OriginLocationId,
    string OriginLocationName,
    Guid DestinationLocationId,
    string DestinationLocationName,
    string LinkType,
    TimeSpan TransitTime,
    bool IsBidirectional,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ========== ROUTE BLUEPRINT DTOs ==========

public record CreateRouteBlueprintRequest(
    string Name,
    string? Description
);

public record UpdateRouteBlueprintRequest(
    string Name,
    string? Description,
    int TotalSteps,
    TimeSpan TotalTransitTime,
    bool IsActive
);

public record RouteBlueprintResponse(
    Guid Id,
    string Name,
    string? Description,
    int TotalSteps,
    TimeSpan TotalTransitTime,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ========== ROUTE STEP DTOs ==========

public record CreateRouteStepRequest(
    Guid RouteBlueprintId,
    Guid LocationId,
    int StepOrder,
    TimeSpan StandardTransitTime,
    string StepType
);

public record UpdateRouteStepRequest(
    Guid LocationId,
    int StepOrder,
    TimeSpan StandardTransitTime,
    string StepType
);

public record RouteStepResponse(
    Guid Id,
    Guid RouteBlueprintId,
    string RouteName,
    Guid LocationId,
    string LocationName,
    int StepOrder,
    TimeSpan StandardTransitTime,
    string StepType,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
