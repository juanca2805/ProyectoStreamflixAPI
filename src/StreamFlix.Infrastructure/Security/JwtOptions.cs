namespace StreamFlix.Infrastructure.Security;

/// <summary>
/// Se bindea desde la sección "Jwt" de la configuración (appsettings.json /
/// variables de entorno). Program.cs usa los mismos valores para configurar
/// la validación del token (AddJwtBearer): la Key tiene que coincidir en
/// ambos lados, o un token firmado aquí no pasaría la validación allá.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 60;
}
