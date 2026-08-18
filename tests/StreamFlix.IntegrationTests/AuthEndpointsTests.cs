using System.Net;
using System.Net.Http.Json;
using StreamFlix.Application.Auth.Dtos;
using Xunit;

namespace StreamFlix.IntegrationTests;

/// <summary>
/// Verifica el flujo de autenticación en sí: que el admin sembrado (AdminUserSeeder,
/// credenciales de "AdminSeed" en appsettings.Development.json) pueda loguearse,
/// y que el resto de la API sea inaccesible sin un token válido de Admin.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class AuthEndpointsTests
{
    private readonly HttpClient _client;

    // Misma StreamFlixApiFactory compartida que MoviesEndpointsTests (ver
    // IntegrationTestCollection): un solo host, un solo Database.Migrate().
    public AuthEndpointsTests(StreamFlixApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ConCredencialesDeAdminValidas_DevuelveTokenYHttp200()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "admin@streamflix.com",
            Password = "Admin123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.Equal("admin@streamflix.com", body.User.Email);
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecta_DevuelveHttp401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "admin@streamflix.com",
            Password = "password-incorrecta"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMovies_SinToken_DevuelveHttp401()
    {
        var response = await _client.GetAsync("/api/movies");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
