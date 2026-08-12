using System.Text.RegularExpressions;

namespace StreamFlix.Domain.Entities;

/// <summary>
/// Representa un usuario de la plataforma.
/// Domain valida el FORMATO del email (regla de negocio: "un email tiene forma X").
/// No calcula el hash de la contraseña aquí: ese es un detalle técnico (algoritmo de hashing)
/// que pertenece a Infrastructure/Application, no a una regla de negocio del dominio.
/// </summary>
public class User
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    private User() { }

    public User(string name, string email, string passwordHash)
    {
        SetName(name);
        SetEmail(email);
        SetPasswordHash(passwordHash);

        Id = Guid.NewGuid();
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre es obligatorio.", nameof(name));

        Name = name.Trim();
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email))
            throw new ArgumentException("El email no tiene un formato válido.", nameof(email));

        Email = email.Trim().ToLowerInvariant();
    }

    private void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("El hash de la contraseña es obligatorio.", nameof(passwordHash));

        PasswordHash = passwordHash;
    }
}
