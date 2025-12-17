using Microsoft.EntityFrameworkCore;
using Parhelion.Application.Auth;
using Parhelion.Application.DTOs.Common;
using Parhelion.Application.DTOs.Core;
using Parhelion.Application.Interfaces;
using Parhelion.Application.Interfaces.Services;
using Parhelion.Domain.Entities;

namespace Parhelion.Infrastructure.Services.Core;

/// <summary>
/// Implementación del servicio de Users.
/// Gestiona usuarios del sistema incluyendo autenticación y password hashing.
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// Inicializa una nueva instancia del servicio de Users.
    /// </summary>
    /// <param name="unitOfWork">Unit of Work para coordinación de repositorios.</param>
    /// <param name="passwordHasher">Servicio de hashing de passwords.</param>
    public UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    /// <inheritdoc />
    public async Task<PagedResult<UserResponse>> GetAllAsync(
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Users.GetPagedAsync(
            request,
            filter: null,
            orderBy: q => q.OrderByDescending(u => u.CreatedAt),
            cancellationToken);

        var dtos = new List<UserResponse>();
        foreach (var user in items)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId, cancellationToken);
            dtos.Add(MapToResponse(user, role?.Name ?? "Unknown"));
        }

        return PagedResult<UserResponse>.From(dtos, totalCount, request);
    }

    /// <inheritdoc />
    public async Task<UserResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (entity == null) return null;

        var role = await _unitOfWork.Roles.GetByIdAsync(entity.RoleId, cancellationToken);
        return MapToResponse(entity, role?.Name ?? "Unknown");
    }

    /// <inheritdoc />
    public async Task<OperationResult<UserResponse>> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validar email único
        var existingByEmail = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => u.Email == request.Email, cancellationToken);
        
        if (existingByEmail != null)
        {
            return OperationResult<UserResponse>.Fail(
                $"Ya existe un usuario con el email '{request.Email}'");
        }

        // Validar que el rol exista
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return OperationResult<UserResponse>.Fail("Rol no encontrado");
        }

        // Hash del password (Argon2id para SuperAdmins, BCrypt para resto)
        var isSuperAdmin = request.Email.EndsWith("@parhelion.com");
        var passwordHash = _passwordHasher.HashPassword(request.Password, isSuperAdmin);

        var entity = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = passwordHash,
            FullName = request.FullName,
            RoleId = request.RoleId,
            IsDemoUser = request.IsDemoUser,
            IsSuperAdmin = isSuperAdmin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<UserResponse>.Ok(
            MapToResponse(entity, role.Name),
            "Usuario creado exitosamente");
    }

    /// <inheritdoc />
    public async Task<OperationResult<UserResponse>> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return OperationResult<UserResponse>.Fail("Usuario no encontrado");
        }

        // Validar que el rol exista
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return OperationResult<UserResponse>.Fail("Rol no encontrado");
        }

        entity.FullName = request.FullName;
        entity.RoleId = request.RoleId;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<UserResponse>.Ok(
            MapToResponse(entity, role.Name),
            "Usuario actualizado exitosamente");
    }

    /// <inheritdoc />
    public async Task<OperationResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return OperationResult.Fail("Usuario no encontrado");
        }

        if (entity.IsSuperAdmin)
        {
            return OperationResult.Fail("No se puede eliminar un Super Admin");
        }

        _unitOfWork.Users.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult.Ok("Usuario eliminado exitosamente");
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserResponse?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => u.Email == email, cancellationToken);
        
        if (entity == null) return null;

        var role = await _unitOfWork.Roles.GetByIdAsync(entity.RoleId, cancellationToken);
        return MapToResponse(entity, role?.Name ?? "Unknown");
    }

    /// <inheritdoc />
    public async Task<PagedResult<UserResponse>> GetByTenantAsync(
        Guid tenantId,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Users.GetPagedAsync(
            request,
            filter: u => u.TenantId == tenantId,
            orderBy: q => q.OrderByDescending(u => u.CreatedAt),
            cancellationToken);

        var dtos = new List<UserResponse>();
        foreach (var user in items)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId, cancellationToken);
            dtos.Add(MapToResponse(user, role?.Name ?? "Unknown"));
        }

        return PagedResult<UserResponse>.From(dtos, totalCount, request);
    }

    /// <inheritdoc />
    public async Task<UserResponse?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => u.Email == email && u.IsActive, cancellationToken);
        
        if (entity == null) return null;

        // Verificar password (detectar si es Argon2id basado en SuperAdmin)
        var isValid = _passwordHasher.VerifyPassword(
            password, entity.PasswordHash, entity.IsSuperAdmin);
        
        if (!isValid) return null;

        var role = await _unitOfWork.Roles.GetByIdAsync(entity.RoleId, cancellationToken);
        return MapToResponse(entity, role?.Name ?? "Unknown");
    }

    /// <inheritdoc />
    public async Task UpdateLastLoginAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (entity == null) return;

        entity.LastLogin = DateTime.UtcNow;
        _unitOfWork.Users.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OperationResult> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (entity == null)
        {
            return OperationResult.Fail("Usuario no encontrado");
        }

        // Verificar password actual
        var isValid = _passwordHasher.VerifyPassword(
            currentPassword, entity.PasswordHash, entity.IsSuperAdmin);
        
        if (!isValid)
        {
            return OperationResult.Fail("Password actual incorrecto");
        }

        // Hash del nuevo password
        entity.PasswordHash = _passwordHasher.HashPassword(newPassword, entity.IsSuperAdmin);
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult.Ok("Password actualizado exitosamente");
    }

    /// <summary>
    /// Mapea una entidad User a su DTO de respuesta.
    /// </summary>
    private static UserResponse MapToResponse(User entity, string roleName) => new(
        entity.Id,
        entity.Email,
        entity.FullName,
        entity.RoleId,
        roleName,
        entity.IsDemoUser,
        entity.IsSuperAdmin,
        entity.LastLogin,
        entity.IsActive,
        entity.CreatedAt,
        entity.UpdatedAt
    );
}
