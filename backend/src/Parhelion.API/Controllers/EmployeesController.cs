using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Application.Interfaces.Services;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de empleados.
/// </summary>
[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    /// <summary>
    /// Inicializa el controlador con el servicio de Employees.
    /// </summary>
    /// <param name="employeeService">Servicio de gestión de empleados.</param>
    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    /// <summary>
    /// Obtiene todos los empleados con paginación.
    /// </summary>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista paginada de empleados.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<EmployeeResponse>>> GetAll(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _employeeService.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un empleado por ID.
    /// </summary>
    /// <param name="id">ID del empleado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Empleado encontrado.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _employeeService.GetByIdAsync(id, cancellationToken);
        if (item == null)
            return NotFound(new { error = "Empleado no encontrado" });

        return Ok(item);
    }

    /// <summary>
    /// Obtiene un empleado por su User ID.
    /// </summary>
    /// <param name="userId">ID del usuario asociado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Empleado encontrado.</returns>
    [HttpGet("by-user/{userId:guid}")]
    public async Task<ActionResult<EmployeeResponse>> GetByUserId(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var item = await _employeeService.GetByUserIdAsync(userId, cancellationToken);
        if (item == null)
            return NotFound(new { error = "Empleado no encontrado" });

        return Ok(item);
    }

    /// <summary>
    /// Busca un empleado por RFC.
    /// </summary>
    /// <param name="rfc">RFC del empleado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Empleado encontrado.</returns>
    [HttpGet("by-rfc/{rfc}")]
    public async Task<ActionResult<EmployeeResponse>> GetByRfc(
        string rfc,
        CancellationToken cancellationToken = default)
    {
        var item = await _employeeService.GetByRfcAsync(rfc, cancellationToken);
        if (item == null)
            return NotFound(new { error = "Empleado no encontrado" });

        return Ok(item);
    }

    /// <summary>
    /// Obtiene empleados por departamento del tenant actual.
    /// </summary>
    /// <param name="department">Nombre del departamento.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista paginada de empleados del departamento.</returns>
    [HttpGet("by-department/{department}")]
    public async Task<ActionResult<PagedResult<EmployeeResponse>>> ByDepartment(
        string department,
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var result = await _employeeService.GetByDepartmentAsync(
            tenantId, department, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene empleados del tenant actual.
    /// </summary>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista paginada de empleados del tenant.</returns>
    [HttpGet("current-tenant")]
    public async Task<ActionResult<PagedResult<EmployeeResponse>>> GetByCurrentTenant(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var result = await _employeeService.GetByTenantAsync(tenantId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo empleado.
    /// </summary>
    /// <param name="request">Datos del nuevo empleado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Empleado creado.</returns>
    [HttpPost]
    public async Task<ActionResult<EmployeeResponse>> Create(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _employeeService.CreateAsync(request, cancellationToken);
        
        if (!result.Success)
            return Conflict(new { error = result.Message });

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            result.Data);
    }

    /// <summary>
    /// Actualiza un empleado existente.
    /// </summary>
    /// <param name="id">ID del empleado.</param>
    /// <param name="request">Datos de actualización.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Empleado actualizado.</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmployeeResponse>> Update(
        Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _employeeService.UpdateAsync(id, request, cancellationToken);
        
        if (!result.Success)
        {
            if (result.Message?.Contains("no encontrado") == true)
                return NotFound(new { error = result.Message });
            return Conflict(new { error = result.Message });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Elimina (soft-delete) un empleado.
    /// </summary>
    /// <param name="id">ID del empleado.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>204 No Content.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _employeeService.DeleteAsync(id, cancellationToken);
        
        if (!result.Success)
            return NotFound(new { error = result.Message });

        return NoContent();
    }
}
