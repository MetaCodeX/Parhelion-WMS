namespace Parhelion.Application.Auth;

/// <summary>
/// Servicio para hashear y verificar passwords.
/// Usa BCrypt para usuarios normales y Argon2id para admin.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Genera un hash del password.
    /// </summary>
    /// <param name="password">Password en texto plano</param>
    /// <param name="useArgon2">True para usar Argon2id (admin), false para BCrypt</param>
    /// <returns>Hash del password</returns>
    string HashPassword(string password, bool useArgon2 = false);
    
    /// <summary>
    /// Verifica si un password coincide con su hash.
    /// </summary>
    /// <param name="password">Password en texto plano</param>
    /// <param name="passwordHash">Hash almacenado</param>
    /// <param name="usesArgon2">True si el hash es Argon2id</param>
    /// <returns>True si coincide</returns>
    bool VerifyPassword(string password, string passwordHash, bool usesArgon2 = false);
}
