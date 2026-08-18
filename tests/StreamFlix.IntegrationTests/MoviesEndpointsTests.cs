using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using StreamFlix.Application.Auth.Dtos;
using StreamFlix.Application.Genres.Dtos;
using StreamFlix.Application.Movies.Dtos;
using Xunit;

namespace StreamFlix.IntegrationTests;

/// <summary>
/// Test de INTEGRACIÓN: a diferencia de los tests unitarios (Fase 12, con mocks),
/// aquí NO se reemplaza nada. WebApplicationFactory arranca la API completa
/// en memoria (con su Program.cs real, su DI real) y las peticiones HTTP viajan
/// por el pipeline entero, incluyendo PostgreSQL real (el mismo contenedor
/// levantado por docker-compose para desarrollo).
///
///   HTTP Request
///     ↓
///   TestServer (host en memoria, mismo pipeline que en producción)
///     ↓
///   MoviesController
///     ↓
///   MovieService (Application)
///     ↓
///   MovieRepository (Infrastructure) → AppDbContext
///     ↓
///   PostgreSQL (contenedor Docker real)
///
/// Nota: si NO se dispusiera de una base de datos real accesible durante los tests
/// (por ejemplo, en un pipeline de CI sin Docker), la alternativa profesional sería
/// usar Testcontainers (https://testcontainers.com/) para levantar un contenedor de
/// PostgreSQL efímero solo para la duración de los tests, o apuntar a una base de
/// datos de test dedicada mediante una cadena de conexión distinta en
/// appsettings.Testing.json.
/// </summary>
public class MoviesEndpointsTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;

    public MoviesEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        }).CreateClient();
    }

    /// <summary>
    /// Todos los endpoints salvo /api/auth/login exigen un token de Admin (ver
    /// Program.cs). Antes de cada test nos logueamos con el admin que
    /// AdminUserSeeder crea automáticamente al arrancar la app, usando las
    /// credenciales de appsettings.Development.json ("AdminSeed"), y dejamos
    /// el token puesto por defecto en el cliente HTTP para el resto de la clase.
    /// </summary>
    public async Task InitializeAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "admin@streamflix.com",
            Password = "Admin123!"
        });
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_DevuelveHttp200()
    {
        var response = await _client.GetAsync("/api/movies");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_CrearGeneroYPelicula_PermiteConsultarlaDespues()
    {
        // 1. Crear un género (setup necesario para crear la película).
        var genreResponse = await _client.PostAsJsonAsync("/api/genres", new CreateGenreRequest
        {
            Name = $"Genero-Test-{Guid.NewGuid():N}"
        });
        genreResponse.EnsureSuccessStatusCode();
        var genre = await genreResponse.Content.ReadFromJsonAsync<GenreDto>();

        // 2. Crear la película usando el género recién creado.
        var createRequest = new CreateMovieRequest
        {
            Title = "Test de integración",
            Description = "Película creada durante un test de integración",
            ReleaseYear = 2023,
            DurationMinutes = 120,
            Rating = 7.5,
            GenreIds = new List<Guid> { genre!.Id }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/movies", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<MovieDto>();
        Assert.NotNull(created);
        Assert.Equal("Test de integración", created!.Title);

        // 3. Consultarla por id: si esto funciona, la petición realmente atravesó
        //    todo el pipeline y persistió en PostgreSQL (no es un mock).
        var getResponse = await _client.GetAsync($"/api/movies/{created.Id}");
        getResponse.EnsureSuccessStatusCode();

        var fetched = await getResponse.Content.ReadFromJsonAsync<MovieDto>();
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Contains(genre.Name, fetched.Genres);
    }

    [Fact]
    public async Task Get_PeliculaInexistente_DevuelveHttp404()
    {
        var response = await _client.GetAsync($"/api/movies/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
