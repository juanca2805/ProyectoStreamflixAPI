namespace StreamFlix.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se busca una entidad por su Id y no existe.
/// Vive en Domain (no en Api) porque es Application/Domain quien SABE que algo
/// no existe; Api solo traduce esta excepción a un código HTTP 404 en el middleware global.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} con id '{id}' no fue encontrado.")
    {
    }
}
