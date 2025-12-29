namespace Parhelion.Application.DTOs.Core;

// ========== TENANT DTOs ==========

public record CreateTenantRequest(
    string CompanyName,
    string ContactEmail,
    int FleetSize,
    int DriverCount
);

public record UpdateTenantRequest(
    string CompanyName,
    string ContactEmail,
    int FleetSize,
    int DriverCount,
    bool IsActive
);

public record TenantResponse(
    Guid Id,
    string CompanyName,
    string ContactEmail,
    int FleetSize,
    int DriverCount,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    /// <summary>
    /// API Key generada automáticamente. Solo se muestra al momento de creación.
    /// </summary>
    string? GeneratedApiKey = null
);

// ========== USER DTOs ==========

public record CreateUserRequest(
    string Email,
    string Password,
    string FullName,
    Guid RoleId,
    bool IsDemoUser = false,
    /// <summary>
    /// Optional: Only SuperAdmin can specify a different TenantId.
    /// For regular admins, this is ignored and the user inherits the admin's TenantId.
    /// </summary>
    Guid? TargetTenantId = null
);

public record UpdateUserRequest(
    string FullName,
    Guid RoleId,
    bool IsActive
);

public record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    Guid RoleId,
    string RoleName,
    bool IsDemoUser,
    bool IsSuperAdmin,
    DateTime? LastLogin,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ========== ROLE DTOs ==========

public record CreateRoleRequest(
    string Name,
    string? Description
);

public record UpdateRoleRequest(
    string Name,
    string? Description
);

public record RoleResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt
);

// ========== EMPLOYEE DTOs ==========

public record CreateEmployeeRequest(
    Guid UserId,
    string Phone,
    string? Rfc,
    string? Nss,
    string? Curp,
    string? EmergencyContact,
    string? EmergencyPhone,
    DateTime? HireDate,
    Guid? ShiftId,
    string? Department
);

public record UpdateEmployeeRequest(
    string Phone,
    string? Rfc,
    string? Nss,
    string? Curp,
    string? EmergencyContact,
    string? EmergencyPhone,
    DateTime? HireDate,
    Guid? ShiftId,
    string? Department
);

public record EmployeeResponse(
    Guid Id,
    Guid UserId,
    string UserFullName,
    string UserEmail,
    string Phone,
    string? Rfc,
    string? Nss,
    string? Curp,
    string? EmergencyContact,
    string? EmergencyPhone,
    DateTime? HireDate,
    Guid? ShiftId,
    string? Department,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ========== CLIENT DTOs ==========

public record CreateClientRequest(
    string CompanyName,
    string? TradeName,
    string ContactName,
    string Email,
    string Phone,
    string? TaxId,
    string? LegalName,
    string? BillingAddress,
    string ShippingAddress,
    string? PreferredProductTypes,
    string Priority,
    string? Notes
);

public record UpdateClientRequest(
    string CompanyName,
    string? TradeName,
    string ContactName,
    string Email,
    string Phone,
    string? TaxId,
    string? LegalName,
    string? BillingAddress,
    string ShippingAddress,
    string? PreferredProductTypes,
    string Priority,
    bool IsActive,
    string? Notes
);

public record ClientResponse(
    Guid Id,
    string CompanyName,
    string? TradeName,
    string ContactName,
    string Email,
    string Phone,
    string? TaxId,
    string? LegalName,
    string? BillingAddress,
    string ShippingAddress,
    string? PreferredProductTypes,
    string Priority,
    bool IsActive,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
