namespace StreamFlix.Api.Authorization;

/// <summary>
/// Nombres de las policies de autorización, en un solo lugar para que los
/// [Authorize(Policy = ...)] de los Controllers y el AddAuthorization de
/// Program.cs no repitan strings sueltos que puedan desincronizarse.
///
/// Modelo de permisos:
///   - Cualquier usuario autenticado (rol User o Admin) puede LEER el catálogo
///     (movies, genres) y gestionar SUS PROPIOS favoritos e historial.
///   - Solo Admin puede ESCRIBIR el catálogo (POST/PUT/DELETE de movies y genres)
///     y administrar usuarios (crear, consultar cualquiera).
/// </summary>
public static class Policies
{
    /// <summary>Exige rol Admin.</summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>
    /// Exige que el {userId} de la ruta coincida con el usuario del token,
    /// o que el usuario sea Admin. Ver OwnerOrAdminHandler.
    /// </summary>
    public const string OwnerOrAdmin = "OwnerOrAdmin";
}
