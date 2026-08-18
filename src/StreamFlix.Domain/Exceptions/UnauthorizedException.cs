namespace StreamFlix.Domain.Exceptions;

/// <summary>
/// Se lanza cuando unas credenciales (email + password) no son válidas al hacer login.
/// A propósito NO distingue "el email no existe" de "la password es incorrecta" en el
/// mensaje: dar esa pista facilitaría enumerar emails registrados probando el endpoint.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
