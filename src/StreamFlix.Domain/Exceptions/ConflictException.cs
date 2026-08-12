namespace StreamFlix.Domain.Exceptions;

/// <summary>
/// Se lanza cuando una operación entra en conflicto con el estado actual de los datos.
/// Ejemplo: marcar como favorita una película que el usuario ya tiene en favoritos.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
