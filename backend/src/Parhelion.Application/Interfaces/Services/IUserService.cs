using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Domain.Entities;

namespace Parhelion.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de Users.
/// Maneja autenticación, roles y estado de usuarios.
/// </summary>
public interface IUserService : IGenericService<User, UserResponse, CreateUserRequest, UpdateUserRequest>
{
    /// <summary>
    /// Busca un usuario por su email.
    /// </summary>
    /// <param name="email">Email del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Usuario encontrado o null.</returns>
    Task<UserResponse?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene usuarios por tenant.
    /// </summary>
    /// <param name="tenantId">ID del tenant.</param>
    /// <param name="request">Parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado paginado de usuarios del tenant.</returns>
    Task<PagedResult<UserResponse>> GetByTenantAsync(
        Guid tenantId,
        PagedRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica credenciales de usuario (para login).
    /// </summary>
    /// <param name="email">Email del usuario.</param>
    /// <param name="password">Password en texto plano.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Usuario si credenciales válidas, null si no.</returns>
    Task<UserResponse?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza el último login del usuario.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task UpdateLastLoginAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cambia el password de un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="currentPassword">Password actual.</param>
    /// <param name="newPassword">Nuevo password.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<OperationResult> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);
}
