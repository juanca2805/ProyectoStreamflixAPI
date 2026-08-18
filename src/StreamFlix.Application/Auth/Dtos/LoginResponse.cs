using StreamFlix.Application.Users.Dtos;

namespace StreamFlix.Application.Auth.Dtos;

/// <summary>
/// Respuesta del login: el token que el cliente debe reenviar en el header
/// "Authorization: Bearer {Token}" en cada petición posterior, más los datos
/// del usuario autenticado para que el cliente no tenga que decodificar el JWT.
/// </summary>
public class LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public UserDto User { get; init; } = null!;
}
