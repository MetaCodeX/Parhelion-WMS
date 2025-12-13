using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parhelion.Application.Auth;
using Parhelion.Application.DTOs.Auth;
using Parhelion.Domain.Entities;
using Parhelion.Infrastructure.Data;

namespace Parhelion.API.Controllers;

/// <summary>
/// Controlador de autenticación.
/// Maneja login, refresh de tokens y logout.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ParhelionDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthController(
        ParhelionDbContext context,
        IJwtService jwtService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Login de usuario con email y password.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        // Buscar usuario por email (ignorar filtro de tenant para login)
        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.Role)
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);

        if (user == null)
        {
            return Unauthorized(new { error = "Email o contraseña incorrectos" });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { error = "Usuario inactivo" });
        }

        // Verificar password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.UsesArgon2))
        {
            return Unauthorized(new { error = "Email o contraseña incorrectos" });
        }

        // Actualizar último login
        user.LastLogin = DateTime.UtcNow;

        // Generar tokens
        var accessToken = _jwtService.GenerateAccessToken(user, user.Role.Name);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Guardar refresh token hasheado
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            ExpiresAt = _jwtService.GetRefreshTokenExpiration(),
            CreatedFromIp = GetClientIp(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _jwtService.GetAccessTokenExpiration(),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.Name,
                TenantId = user.TenantId
            }
        });
    }

    /// <summary>
    /// Renovar access token usando refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var tokenHash = HashToken(request.RefreshToken);

        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt => 
                rt.TokenHash == tokenHash && 
                !rt.IsRevoked && 
                rt.ExpiresAt > DateTime.UtcNow);

        if (refreshToken == null)
        {
            return Unauthorized(new { error = "Refresh token inválido o expirado" });
        }

        var user = refreshToken.User;
        if (!user.IsActive || user.IsDeleted)
        {
            return Unauthorized(new { error = "Usuario inactivo" });
        }

        // Revocar token actual
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedReason = "Replaced";

        // Generar nuevos tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user, user.Role.Name);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Guardar nuevo refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(newRefreshToken),
            ExpiresAt = _jwtService.GetRefreshTokenExpiration(),
            CreatedFromIp = GetClientIp(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = _jwtService.GetAccessTokenExpiration(),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.Name,
                TenantId = user.TenantId
            }
        });
    }

    /// <summary>
    /// Logout - revoca el refresh token.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var tokenHash = HashToken(request.RefreshToken);

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && !rt.IsRevoked);

        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedReason = "Logout";
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "Sesión cerrada correctamente" });
    }

    /// <summary>
    /// Obtener información del usuario actual.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfo>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new UserInfo
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.Name,
            TenantId = user.TenantId
        });
    }

    // ========== Private Helpers ==========

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    private string GetClientIp()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
