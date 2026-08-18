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

    public bool Verify(string plainPassword, string passwordHash)
    {
        var parts = passwordHash.Split('.', 3);
        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[0], out var iterations))
            return false;

        var salt = Convert.FromBase64String(parts[1]);
        var expectedKey = Convert.FromBase64String(parts[2]);

        var actualKey = Rfc2898DeriveBytes.Pbkdf2(
            plainPassword,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedKey.Length);

        // Comparación en tiempo constante: con "==" o SequenceEqual, el tiempo que
        // tarda la comparación varía según cuántos bytes coinciden antes del primer
        // byte distinto, lo que en teoría permite a un atacante deducir el hash
        // byte a byte midiendo tiempos de respuesta (timing attack).
        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }
}
