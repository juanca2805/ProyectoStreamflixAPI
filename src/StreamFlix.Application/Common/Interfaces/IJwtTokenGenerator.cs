using StreamFlix.Domain.Entities;

namespace StreamFlix.Application.Common.Interfaces;

/// <summary>
/// Igual que con IPasswordHasher: Application sabe que necesita "un token para
/// este usuario ya autenticado", pero el formato concreto (JWT), la librería usada
/// para firmarlo y la clave secreta son detalles técnicos que pertenecen a Infrastructure.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Genera un JWT firmado para el usuario dado, incluyendo su Id, email y rol
    /// como claims, junto con la fecha de expiración usada al firmarlo.
    /// </summary>
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
