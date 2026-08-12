using StreamFlix.Domain.Entities;

namespace StreamFlix.Application.Users.Dtos;

/// <summary>
/// DTO de respuesta de usuario. A propósito NO incluye PasswordHash:
/// aunque esté hasheado, no hay ninguna razón para que viaje por HTTP hacia el cliente.
/// Esta es la razón principal por la que NUNCA se deben exponer entidades directamente:
/// una entidad User completa incluiría el hash sin que nadie lo decida explícitamente.
/// </summary>
public class UserDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    public static UserDto FromEntity(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email
    };
}
