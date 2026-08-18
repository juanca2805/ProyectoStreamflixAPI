namespace StreamFlix.Domain.Entities;

/// <summary>
/// Rol de un usuario dentro de la plataforma. Determina, entre otras cosas,
/// qué endpoints puede consultar una vez autenticado con JWT (ver Fase JWT):
/// por ahora, solo los usuarios con rol Admin pueden operar la API.
/// </summary>
public enum UserRole
{
    User = 0,
    Admin = 1
}
