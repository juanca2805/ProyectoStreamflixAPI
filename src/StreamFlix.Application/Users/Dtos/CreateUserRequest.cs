namespace StreamFlix.Application.Users.Dtos;

/// <summary>
/// DTO de entrada para crear un usuario. Contiene "Password" en texto plano
/// (tal como lo escribe el usuario en un formulario) porque el hashing
/// ocurre DESPUÉS, dentro de UserService, antes de construir la entidad User.
/// </summary>
public class CreateUserRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
