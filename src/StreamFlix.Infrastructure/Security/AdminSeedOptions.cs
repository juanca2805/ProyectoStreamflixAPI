namespace StreamFlix.Infrastructure.Security;

/// <summary>
/// Se bindea desde la sección "AdminSeed" de la configuración. Define el único
/// usuario Admin que la app puede crear por sí misma al arrancar: como todos los
/// endpoints exigen un token de Admin (ver Program.cs), sin esto nadie podría
/// loguearse nunca y la API quedaría inaccesible desde cero.
/// </summary>
public class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string Name { get; init; } = "Administrador";
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
