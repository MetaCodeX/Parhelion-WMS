using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;
using Parhelion.Application.Auth;

namespace Parhelion.Infrastructure.Services.Core;

/// <summary>
/// Implementación del servicio de Roles.
/// Gestiona roles del sistema con permisos inmutables definidos en RolePermissions.
/// </summary>
public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Inicializa una nueva instancia del servicio de Roles.
    /// </summary>
    /// <param name="unitOfWork">Unit of Work para coordinación de repositorios.</param>
    public RoleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<PagedResult<RoleResponse>> GetAllAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Roles.GetPagedAsync(
            request,
            filter: null,
            orderBy: q => q.OrderBy(r => r.Name),
            cancellationToken);

        var dtos = items.Select(MapToResponse);
        return PagedResult<RoleResponse>.From(dtos, totalCount, request);
    }

    /// <inheritdoc />
    public async Task<RoleResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Roles.GetByIdAsync(id, cancellationToken);
        return entity != null ? MapToResponse(entity) : null;
    }

    /// <inheritdoc />
    public async Task<OperationResult<RoleResponse>> CreateAsync(
        CreateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validar nombre único
        var existingByName = await _unitOfWork.Roles.FirstOrDefaultAsync(
            r => r.Name == request.Name, cancellationToken);
        
        if (existingByName != null)
        {
            return OperationResult<RoleResponse>.Fail(
                $"Ya existe un rol con el nombre '{request.Name}'");
        }

        var entity = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Roles.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<RoleResponse>.Ok(
            MapToResponse(entity),
            "Rol creado exitosamente");
    }

    /// <inheritdoc />
    public async Task<OperationResult<RoleResponse>> UpdateAsync(
        Guid id,
        UpdateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Roles.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return OperationResult<RoleResponse>.Fail("Rol no encontrado");
        }

        // Validar nombre único (excluyendo el actual)
        var existingByName = await _unitOfWork.Roles.FirstOrDefaultAsync(
            r => r.Name == request.Name && r.Id != id, cancellationToken);
        
        if (existingByName != null)
        {
            return OperationResult<RoleResponse>.Fail(
                $"Ya existe otro rol con el nombre '{request.Name}'");
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Roles.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<RoleResponse>.Ok(
            MapToResponse(entity),
            "Rol actualizado exitosamente");
    }

    /// <inheritdoc />
    public async Task<OperationResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Roles.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return OperationResult.Fail("Rol no encontrado");
        }

        // Verificar si hay usuarios con este rol
        var usersWithRole = await _unitOfWork.Users.AnyAsync(
            u => u.RoleId == id, cancellationToken);
        
        if (usersWithRole)
        {
            return OperationResult.Fail(
                "No se puede eliminar el rol porque tiene usuarios asignados");
        }

        _unitOfWork.Roles.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult.Ok("Rol eliminado exitosamente");
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Roles.AnyAsync(r => r.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RoleResponse?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Roles.FirstOrDefaultAsync(
            r => r.Name == name, cancellationToken);
        return entity != null ? MapToResponse(entity) : null;
    }

    /// <inheritdoc />
    public IEnumerable<Permission> GetPermissions(string roleName)
    {
        return RolePermissions.GetPermissions(roleName);
    }

    /// <inheritdoc />
    public bool HasPermission(string roleName, Permission permission)
    {
        return RolePermissions.HasPermission(roleName, permission);
    }

    /// <summary>
    /// Mapea una entidad Role a su DTO de respuesta.
    /// </summary>
    private static RoleResponse MapToResponse(Role entity) => new(
        entity.Id,
        entity.Name,
        entity.Description,
        entity.CreatedAt
    );
}
