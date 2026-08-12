namespace StreamFlix.Application.Common.Interfaces;

/// <summary>
/// Igual que con los repositorios: Application sabe que necesita "hashear una contraseña",
/// pero el algoritmo concreto (PBKDF2, BCrypt, Argon2...) es un detalle técnico
/// que pertenece a Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string plainPassword);
}
