using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Domain.Entities;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de Employees (datos laborales de usuarios).
/// Un Employee está vinculado 1:1 con un User y contiene datos legales (RFC, NSS, CURP).
/// </summary>
public interface IEmployeeService : IGenericService<Employee, EmployeeResponse, CreateEmployeeRequest, UpdateEmployeeRequest>
{
    /// <summary>
    /// Obtiene un empleado por su User ID.
    /// </summary>
    /// <param name="userId">ID del usuario asociado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Empleado encontrado o null.</returns>
    Task<EmployeeResponse?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene empleados por tenant.
    /// </summary>
    /// <param name="tenantId">ID del tenant.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de empleados del tenant.</returns>
    Task<PagedResult<EmployeeResponse>> GetByTenantAsync(
        Guid tenantId,
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca un empleado por RFC.
    /// </summary>
    /// <param name="rfc">RFC del empleado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Empleado encontrado o null.</returns>
    Task<EmployeeResponse?> GetByRfcAsync(
        string rfc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene empleados por departamento.
    /// </summary>
    /// <param name="tenantId">ID del tenant.</param>
    /// <param name="department">Nombre del departamento.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de empleados del departamento.</returns>
    Task<PagedResult<EmployeeResponse>> GetByDepartmentAsync(
        Guid tenantId,
        string department,
        PagedRequest request,
        CancellationToken cancellationToken = default);
}
