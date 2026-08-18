using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StreamFlix.Application.Auth.Dtos;
using StreamFlix.Application.Genres.Dtos;
using StreamFlix.Application.Users.Dtos;
using Xunit;

namespace StreamFlix.IntegrationTests;

/// <summary>
/// Verifica la matriz de permisos User vs Admin (ver Api/Authorization/Policies.cs):
///   - User: puede LEER el catálogo y gestionar SUS favoritos/historial/perfil.
///   - User: NO puede escribir el catálogo, ni crear usuarios, ni tocar datos de otro.
///   - Admin: puede todo lo anterior.
///
/// Setup: con el token del admin sembrado se crea un usuario normal (rol User por
/// defecto en UserService.CreateAsync) y se loguea para obtener SU token.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class RolesAuthorizationTests : IAsyncLifetime
{
    private readonly StreamFlixApiFactory _factory;
    private HttpClient _adminClient = null!;
    private HttpClient _userClient = null!;
    private Guid _userId;

    public RolesAuthorizationTests(StreamFlixApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _adminClient = await LoginAsync("admin@streamflix.com", "Admin123!");

        var email = $"user-{Guid.NewGuid():N}@example.com";
        var created = await _adminClient.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            Name = "Usuario Normal",
            Email = email,
            Password = "Usuario123!"
        });
        created.EnsureSuccessStatusCode();
        _userId = (await created.Content.ReadFromJsonAsync<UserDto>())!.Id;

        _userClient = await LoginAsync(email, "Usuario123!");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<HttpClient> LoginAsync(string email, string password)
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = password });
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
        return client;
    }

    // --- User: lectura del catálogo permitida ---

    [Fact]
    public async Task User_PuedeLeerElCatalogo()
    {
        Assert.Equal(HttpStatusCode.OK, (await _userClient.GetAsync("/api/movies")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _userClient.GetAsync("/api/genres")).StatusCode);
    }

    // --- User: escritura del catálogo prohibida (403, no 401: está autenticado, pero sin permiso) ---

    [Fact]
    public async Task User_NoPuedeCrearGeneros_DevuelveHttp403()
    {
        var response = await _userClient.PostAsJsonAsync("/api/genres", new CreateGenreRequest { Name = $"X-{Guid.NewGuid():N}" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task User_NoPuedeBorrarPeliculas_DevuelveHttp403()
    {
        var response = await _userClient.DeleteAsync($"/api/movies/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task User_NoPuedeCrearUsuarios_DevuelveHttp403()
    {
        var response = await _userClient.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            Name = "Otro",
            Email = $"otro-{Guid.NewGuid():N}@example.com",
            Password = "Otro123!"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // --- User: datos propios sí, datos ajenos no ---

    [Fact]
    public async Task User_PuedeVerSusPropiosFavoritosYPerfil()
    {
        Assert.Equal(HttpStatusCode.OK, (await _userClient.GetAsync($"/api/users/{_userId}/favorites")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _userClient.GetAsync($"/api/users/{_userId}/history")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _userClient.GetAsync($"/api/users/{_userId}")).StatusCode);
    }

    [Fact]
    public async Task User_NoPuedeVerFavoritosDeOtroUsuario_DevuelveHttp403()
    {
        var otroUserId = Guid.NewGuid();

        var response = await _userClient.GetAsync($"/api/users/{otroUserId}/favorites");

        // 403 y no 404: la autorización corre ANTES que el controller, así que ni
        // siquiera se llega a consultar si ese otro usuario existe.
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // --- Admin: puede tocar datos de cualquiera ---

    [Fact]
    public async Task Admin_PuedeVerFavoritosDeCualquierUsuario()
    {
        var response = await _adminClient.GetAsync($"/api/users/{_userId}/favorites");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
