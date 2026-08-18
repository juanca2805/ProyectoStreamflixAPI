using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace StreamFlix.IntegrationTests;

/// <summary>
/// Fuerza el entorno "Development" (appsettings.Development.json) para que los
/// tests usen la misma cadena de conexión y el mismo "AdminSeed" que en local/Docker.
/// Ver IntegrationTestCollection: todas las clases de test de integración comparten
/// UNA sola instancia de esta factory (y por lo tanto, un solo host levantado una
/// sola vez), en vez de que cada clase cree la suya.
/// </summary>
public class StreamFlixApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
}
