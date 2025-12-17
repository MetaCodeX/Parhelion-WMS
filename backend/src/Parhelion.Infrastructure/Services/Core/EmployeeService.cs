using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Services.Core;

/// <summary>
/// Implementación del servicio de Employees.
/// Gestiona datos laborales de empleados (RFC, NSS, CURP, contacto de emergencia).
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Inicializa una nueva instancia del servicio de Employees.
    /// </summary>
    /// <param name="unitOfWork">Unit of Work para coordinación de repositorios.</param>
    public EmployeeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<PagedResult<EmployeeResponse>> GetAllAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Employees.GetPagedAsync(
            request,
            filter: null,
            orderBy: q => q.OrderByDescending(e => e.CreatedAt),
            cancellationToken);

        var dtos = new List<EmployeeResponse>();
        foreach (var employee in items)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(employee.UserId, cancellationToken);
            dtos.Add(MapToResponse(employee, user));
        }

        return PagedResult<EmployeeResponse>.From(dtos, totalCount, request);
    }

    /// <inheritdoc />
    public async Task<EmployeeResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Employees.GetByIdAsync(id, cancellationToken);
        if (entity == null) return null;

        var user = await _unitOfWork.Users.GetByIdAsync(entity.UserId, cancellationToken);
        return MapToResponse(entity, user);
    }

    /// <inheritdoc />
    public async Task<OperationResult<EmployeeResponse>> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validar que el usuario exista
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return OperationResult<EmployeeResponse>.Fail("Usuario no encontrado");
        }

        // Validar que el usuario no tenga ya un empleado
        var existingByUser = await _unitOfWork.Employees.FirstOrDefaultAsync(
            e => e.UserId == request.UserId, cancellationToken);
        
        if (existingByUser != null)
        {
            return OperationResult<EmployeeResponse>.Fail(
                "El usuario ya tiene un registro de empleado");
        }

        // Validar RFC único si se proporciona
        if (!string.IsNullOrEmpty(request.Rfc))
        {
            var existingByRfc = await _unitOfWork.Employees.FirstOrDefaultAsync(
                e => e.Rfc == request.Rfc, cancellationToken);
            
            if (existingByRfc != null)
            {
                return OperationResult<EmployeeResponse>.Fail(
                    $"Ya existe un empleado con el RFC '{request.Rfc}'");
            }
        }

        var entity = new Employee
        {
            Id = Guid.NewGuid(),
            TenantId = user.TenantId,
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

        await _unitOfWork.Employees.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<EmployeeResponse>.Ok(
            MapToResponse(entity, user),
            "Empleado creado exitosamente");
    }

    /// <inheritdoc />
    public async Task<OperationResult<EmployeeResponse>> UpdateAsync(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Employees.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return OperationResult<EmployeeResponse>.Fail("Empleado no encontrado");
        }

        // Validar RFC único (excluyendo el actual)
        if (!string.IsNullOrEmpty(request.Rfc))
        {
            var existingByRfc = await _unitOfWork.Employees.FirstOrDefaultAsync(
                e => e.Rfc == request.Rfc && e.Id != id, cancellationToken);
            
            if (existingByRfc != null)
            {
                return OperationResult<EmployeeResponse>.Fail(
                    $"Ya existe otro empleado con el RFC '{request.Rfc}'");
            }
        }

        entity.Phone = request.Phone;
        entity.Rfc = request.Rfc;
        entity.Nss = request.Nss;
        entity.Curp = request.Curp;
        entity.EmergencyContact = request.EmergencyContact;
        entity.EmergencyPhone = request.EmergencyPhone;
        entity.HireDate = request.HireDate;
        entity.ShiftId = request.ShiftId;
        entity.Department = request.Department;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Employees.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _unitOfWork.Users.GetByIdAsync(entity.UserId, cancellationToken);
        return OperationResult<EmployeeResponse>.Ok(
            MapToResponse(entity, user),
            "Empleado actualizado exitosamente");
    }

    /// <inheritdoc />
    public async Task<OperationResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Employees.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return OperationResult.Fail("Empleado no encontrado");
        }

        _unitOfWork.Employees.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult.Ok("Empleado eliminado exitosamente");
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Employees.AnyAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<EmployeeResponse?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Employees.FirstOrDefaultAsync(
            e => e.UserId == userId, cancellationToken);
        
        if (entity == null) return null;

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        return MapToResponse(entity, user);
    }

    /// <inheritdoc />
    public async Task<PagedResult<EmployeeResponse>> GetByTenantAsync(
        Guid tenantId,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Employees.GetPagedAsync(
            request,
            filter: e => e.TenantId == tenantId,
            orderBy: q => q.OrderByDescending(e => e.CreatedAt),
            cancellationToken);

        var dtos = new List<EmployeeResponse>();
        foreach (var employee in items)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(employee.UserId, cancellationToken);
            dtos.Add(MapToResponse(employee, user));
        }

        return PagedResult<EmployeeResponse>.From(dtos, totalCount, request);
    }

    /// <inheritdoc />
    public async Task<EmployeeResponse?> GetByRfcAsync(
        string rfc,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Employees.FirstOrDefaultAsync(
            e => e.Rfc == rfc, cancellationToken);
        
        if (entity == null) return null;

        var user = await _unitOfWork.Users.GetByIdAsync(entity.UserId, cancellationToken);
        return MapToResponse(entity, user);
    }

    /// <inheritdoc />
    public async Task<PagedResult<EmployeeResponse>> GetByDepartmentAsync(
        Guid tenantId,
        string department,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Employees.GetPagedAsync(
            request,
            filter: e => e.TenantId == tenantId && e.Department == department,
            orderBy: q => q.OrderByDescending(e => e.CreatedAt),
            cancellationToken);

        var dtos = new List<EmployeeResponse>();
        foreach (var employee in items)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(employee.UserId, cancellationToken);
            dtos.Add(MapToResponse(employee, user));
        }

        return PagedResult<EmployeeResponse>.From(dtos, totalCount, request);
    }

    /// <summary>
    /// Mapea una entidad Employee a su DTO de respuesta.
    /// </summary>
    private static EmployeeResponse MapToResponse(Employee entity, User? user) => new(
        entity.Id,
        entity.UserId,
        user?.FullName ?? "Unknown",
        user?.Email ?? "Unknown",
        entity.Phone,
        entity.Rfc,
        entity.Nss,
        entity.Curp,
        entity.EmergencyContact,
        entity.EmergencyPhone,
        entity.HireDate,
        entity.ShiftId,
        entity.Department,
        entity.CreatedAt,
        entity.UpdatedAt
    );
}
