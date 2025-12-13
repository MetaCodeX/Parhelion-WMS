using System.ComponentModel.DataAnnotations;

namespace Parhelion.Application.DTOs.Auth;

/// <summary>
/// Request para login de usuario.
/// </summary>
public record LoginRequest
{
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; init; } = null!;
    
    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    public string Password { get; init; } = null!;
}

/// <summary>
/// Response de login exitoso.
/// </summary>
public record LoginResponse
{
    /// <summary>JWT Access Token</summary>
    public string AccessToken { get; init; } = null!;
    
    /// <summary>Refresh Token para renovar el access token</summary>
    public string RefreshToken { get; init; } = null!;
    
    /// <summary>Tipo de token (Bearer)</summary>
    public string TokenType { get; init; } = "Bearer";
    
    /// <summary>Fecha de expiración del access token</summary>
    public DateTime ExpiresAt { get; init; }
    
    /// <summary>Información del usuario autenticado</summary>
    public UserInfo User { get; init; } = null!;
}

/// <summary>
/// Información básica del usuario en response de login.
/// </summary>
public record UserInfo
{
    public Guid Id { get; init; }
    public string Email { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string Role { get; init; } = null!;
    public Guid TenantId { get; init; }
}

/// <summary>
/// Request para renovar el access token.
/// </summary>
public record RefreshTokenRequest
{
    [Required(ErrorMessage = "El refresh token es requerido")]
    public string RefreshToken { get; init; } = null!;
}
