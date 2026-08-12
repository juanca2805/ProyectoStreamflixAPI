using Microsoft.Extensions.DependencyInjection;
using StreamFlix.Application.Favorites;
using StreamFlix.Application.Genres;
using StreamFlix.Application.Movies;
using StreamFlix.Application.Users;
using StreamFlix.Application.WatchHistories;

namespace StreamFlix.Application;

/// <summary>
/// Registra los servicios de aplicación (casos de uso) en el contenedor de DI.
/// Se usa AddScoped porque cada servicio depende de repositorios que envuelven
/// a AppDbContext, y AppDbContext está registrado como Scoped (una instancia
/// por petición HTTP) — mezclar tiempos de vida distintos causaría errores.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<MovieService>();
        services.AddScoped<GenreService>();
        services.AddScoped<UserService>();
        services.AddScoped<FavoriteService>();
        services.AddScoped<WatchHistoryService>();

        return services;
    }
}
