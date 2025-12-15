namespace Parhelion.Application.DTOs.Fleet;

// ========== TRUCK DTOs ==========

public record CreateTruckRequest(
    string Plate,
    string Model,
    string Type,
    decimal MaxCapacityKg,
    decimal MaxVolumeM3,
    string? Vin,
    string? EngineNumber,
    int? Year,
    string? Color,
    string? InsurancePolicy,
    DateTime? InsuranceExpiration,
    string? VerificationNumber,
    DateTime? VerificationExpiration
);

public record UpdateTruckRequest(
    string Plate,
    string Model,
    string Type,
    decimal MaxCapacityKg,
    decimal MaxVolumeM3,
    bool IsActive,
    string? Vin,
    string? EngineNumber,
    int? Year,
    string? Color,
    string? InsurancePolicy,
    DateTime? InsuranceExpiration,
    string? VerificationNumber,
    DateTime? VerificationExpiration,
    DateTime? LastMaintenanceDate,
    DateTime? NextMaintenanceDate,
    decimal? CurrentOdometerKm
);

public record TruckResponse(
    Guid Id,
    string Plate,
    string Model,
    string Type,
    decimal MaxCapacityKg,
    decimal MaxVolumeM3,
    bool IsActive,
    string? Vin,
    string? EngineNumber,
    int? Year,
    string? Color,
    string? InsurancePolicy,
    DateTime? InsuranceExpiration,
    string? VerificationNumber,
    DateTime? VerificationExpiration,
    DateTime? LastMaintenanceDate,
    DateTime? NextMaintenanceDate,
    decimal? CurrentOdometerKm,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ========== DRIVER DTOs ==========

public record CreateDriverRequest(
    Guid EmployeeId,
    string LicenseNumber,
    string? LicenseType,
    DateTime? LicenseExpiration,
    Guid? DefaultTruckId,
    string Status
);

public record UpdateDriverRequest(
    string LicenseNumber,
    string? LicenseType,
    DateTime? LicenseExpiration,
    Guid? DefaultTruckId,
    Guid? CurrentTruckId,
    string Status
);

public record DriverResponse(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string LicenseNumber,
    string? LicenseType,
    DateTime? LicenseExpiration,
    Guid? DefaultTruckId,
    string? DefaultTruckPlate,
    Guid? CurrentTruckId,
    string? CurrentTruckPlate,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ========== SHIFT DTOs ==========

public record CreateShiftRequest(
    string Name,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string DaysOfWeek
);

public record UpdateShiftRequest(
    string Name,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string DaysOfWeek,
    bool IsActive
);

public record ShiftResponse(
    Guid Id,
    string Name,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string DaysOfWeek,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ========== FLEET LOG DTOs ==========

public record CreateFleetLogRequest(
    Guid DriverId,
    Guid? OldTruckId,
    Guid NewTruckId,
    string Reason
);

public record FleetLogResponse(
    Guid Id,
    Guid DriverId,
    string DriverName,
    Guid? OldTruckId,
    string? OldTruckPlate,
    Guid NewTruckId,
    string NewTruckPlate,
    string Reason,
    DateTime Timestamp,
    Guid CreatedByUserId,
    string CreatedByName,
    DateTime CreatedAt
);
