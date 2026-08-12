using System.Security.Cryptography;
using StreamFlix.Application.Common.Interfaces;

namespace StreamFlix.Infrastructure.Security;

/// <summary>
/// Implementación de IPasswordHasher usando PBKDF2 (System.Security.Cryptography,
/// estándar de .NET, sin librerías externas). Formato almacenado:
/// {iteraciones}.{salt en base64}.{hash en base64}
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public string Hash(string plainPassword)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        var key = Rfc2898DeriveBytes.Pbkdf2(
            plainPassword,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }
}
