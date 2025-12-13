using Parhelion.Domain.Entities;
using System.Security.Claims;

namespace Parhelion.Application.Auth;

/// <summary>
/// Servicio para generación y validación de JWT tokens.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Genera un token JWT para un usuario autenticado.
    /// </summary>
    /// <param name="user">Usuario autenticado</param>
    /// <param name="roleName">Nombre del rol del usuario</param>
    /// <returns>Token JWT como string</returns>
    string GenerateAccessToken(User user, string roleName);
    
    /// <summary>
    /// Genera un refresh token para renovar el access token.
    /// </summary>
    /// <returns>Refresh token como string</returns>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Valida un token JWT y extrae los claims.
    /// </summary>
    /// <param name="token">Token a validar</param>
    /// <returns>ClaimsPrincipal si es válido, null si no</returns>
    ClaimsPrincipal? ValidateAccessToken(string token);
    
    /// <summary>
    /// Obtiene la fecha de expiración del access token.
    /// </summary>
    DateTime GetAccessTokenExpiration();
    
    /// <summary>
    /// Obtiene la fecha de expiración del refresh token.
    /// </summary>
    DateTime GetRefreshTokenExpiration();
}
