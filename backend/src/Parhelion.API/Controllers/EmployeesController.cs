using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.DTOs.Core;
using Parhelion.Domain.Entities;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador para gestión de empleados.
/// </summary>
[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly ParhelionDbContext _context;

    public EmployeesController(ParhelionDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todos los empleados del tenant.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeResponse>>> GetAll()
    {
        var items = await _context.Employees
            .Include(x => x.User)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.User.FullName)
            .Select(x => MapToResponse(x))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Obtiene un empleado por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeResponse>> GetById(Guid id)
    {
        var item = await _context.Employees
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
            return NotFound(new { error = "Empleado no encontrado" });

        return Ok(MapToResponse(item));
    }

    /// <summary>
    /// Obtiene empleados por departamento.
    /// </summary>
    [HttpGet("by-department/{department}")]
    public async Task<ActionResult<IEnumerable<EmployeeResponse>>> ByDepartment(string department)
    {
        var items = await _context.Employees
            .Include(x => x.User)
            .Where(x => !x.IsDeleted && x.Department == department)
            .OrderBy(x => x.User.FullName)
            .Select(x => MapToResponse(x))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Crea un nuevo empleado.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EmployeeResponse>> Create([FromBody] CreateEmployeeRequest request)
    {
        var existingUser = await _context.Employees
            .AnyAsync(x => x.UserId == request.UserId && !x.IsDeleted);

        if (existingUser)
            return Conflict(new { error = "Este usuario ya tiene un registro de empleado" });

        var tenantIdClaim = User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            return Unauthorized(new { error = "No se pudo determinar el tenant" });

        var item = new Employee
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = request.UserId,
            Phone = request.Phone,
            Rfc = request.Rfc,
            Nss = request.Nss,
            Curp = request.Curp,
            EmergencyContact = request.EmergencyContact,
            EmergencyPhone = request.EmergencyPhone,
            HireDate = request.HireDate,
            ShiftId = request.ShiftId,
            Department = request.Department,
            CreatedAt = DateTime.UtcNow
        };

        _context.Employees.Add(item);
        await _context.SaveChangesAsync();

        item = await _context.Employees.Include(x => x.User).FirstAsync(x => x.Id == item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    /// <summary>
    /// Actualiza un empleado existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmployeeResponse>> Update(Guid id, [FromBody] UpdateEmployeeRequest request)
    {
        var item = await _context.Employees
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
            return NotFound(new { error = "Empleado no encontrado" });

        item.Phone = request.Phone;
        item.Rfc = request.Rfc;
        item.Nss = request.Nss;
        item.Curp = request.Curp;
        item.EmergencyContact = request.EmergencyContact;
        item.EmergencyPhone = request.EmergencyPhone;
        item.HireDate = request.HireDate;
        item.ShiftId = request.ShiftId;
        item.Department = request.Department;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(item));
    }

    /// <summary>
    /// Elimina (soft-delete) un empleado.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var item = await _context.Employees
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (item == null)
            return NotFound(new { error = "Empleado no encontrado" });

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static EmployeeResponse MapToResponse(Employee x) => new(
        x.Id, x.UserId, x.User?.FullName ?? "", x.User?.Email ?? "",
        x.Phone, x.Rfc, x.Nss, x.Curp, x.EmergencyContact, x.EmergencyPhone,
        x.HireDate, x.ShiftId, x.Department, x.CreatedAt, x.UpdatedAt
    );
}
