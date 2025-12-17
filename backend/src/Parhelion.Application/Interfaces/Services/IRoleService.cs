using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Domain.Entities;
using Parhelion.Domain.Enums;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de Roles del sistema.
/// Los roles definen permisos inmutables en código (RolePermissions.cs).
/// </summary>
public interface IRoleService : IGenericService<Role, RoleResponse, CreateRoleRequest, UpdateRoleRequest>
{
    /// <summary>
    /// Busca un rol por su nombre.
    /// </summary>
    /// <param name="name">Nombre del rol (ej: "Admin", "Driver").</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Rol encontrado o null.</returns>
    Task<RoleResponse?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los permisos asociados a un rol.
    /// Los permisos están definidos en RolePermissions.cs (inmutables).
    /// </summary>
    /// <param name="roleName">Nombre del rol.</param>
    /// <returns>Lista de permisos del rol.</returns>
    IEnumerable<Permission> GetPermissions(string roleName);

    /// <summary>
    /// Verifica si un rol tiene un permiso específico.
    /// </summary>
    /// <param name="roleName">Nombre del rol.</param>
    /// <param name="permission">Permiso a verificar.</param>
    /// <returns>True si el rol tiene el permiso.</returns>
    bool HasPermission(string roleName, Permission permission);
}
