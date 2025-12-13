using Parhelion.Application.Auth;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Parhelion.Infrastructure.Auth;

/// <summary>
/// Implementación del servicio de hashing de passwords.
/// Usa BCrypt para usuarios normales.
/// Nota: Argon2id se puede agregar después con el paquete Isopoh.Cryptography.Argon2
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12; // 2^12 = 4096 iterations

    /// <inheritdoc />
    public string HashPassword(string password, bool useArgon2 = false)
    {
        if (useArgon2)
        {
            // TODO: Implementar Argon2id para admin cuando se agregue el paquete
            // Por ahora usar BCrypt con mayor work factor
            return BCryptNet.HashPassword(password, BCryptNet.GenerateSalt(14));
        }
        
        return BCryptNet.HashPassword(password, BCryptNet.GenerateSalt(WorkFactor));
    }

    /// <inheritdoc />
    public bool VerifyPassword(string password, string passwordHash, bool usesArgon2 = false)
    {
        try
        {
            if (usesArgon2)
            {
                // TODO: Verificar con Argon2id cuando se implemente
                // Por ahora verificar con BCrypt
                return BCryptNet.Verify(password, passwordHash);
            }
            
            return BCryptNet.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}
