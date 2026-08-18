namespace StreamFlix.Application.Common.Interfaces;

/// <summary>
/// Igual que con los repositorios: Application sabe que necesita "hashear una contraseña",
/// pero el algoritmo concreto (PBKDF2, BCrypt, Argon2...) es un detalle técnico
/// que pertenece a Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string plainPassword);

    /// <summary>
    /// Verifica que una contraseña en texto plano corresponda al hash almacenado.
    /// Necesario para el login: no se puede "deshacer" un hash, solo volver a
    /// calcularlo con la misma sal y compararlo contra el que ya está guardado.
    /// </summary>
    bool Verify(string plainPassword, string passwordHash);
}
