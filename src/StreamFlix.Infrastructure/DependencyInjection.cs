using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Infrastructure.Persistence;
using StreamFlix.Infrastructure.Repositories;
using StreamFlix.Infrastructure.Security;

namespace StreamFlix.Infrastructure;

/// <summary>
/// Punto único donde Infrastructure "se presenta" ante Api: expone un método de
/// extensión que registra el DbContext, los repositorios concretos y los servicios
/// técnicos (como el password hasher) en el contenedor de inyección de dependencias.
///
/// Esto evita que Program.cs tenga que conocer los tipos concretos de Infrastructure
/// uno por uno; solo llama a builder.Services.AddInfrastructure(configuration).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'DefaultConnection' en la configuración.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWatchHistoryRepository, WatchHistoryRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        // "Jwt" y "AdminSeed" se bindean acá (no en Program.cs) por el mismo motivo
        // que el resto de este archivo: Api no necesita conocer JwtOptions ni
        // AdminSeedOptions, solo pedir "un IJwtTokenGenerator" o "un AdminUserSeeder".
        // Program.cs sí vuelve a leer "Jwt:Key/Issuer/Audience" directamente de la
        // configuración al configurar AddJwtBearer, porque la validación del token
        // (a diferencia de generarlo) es responsabilidad del pipeline HTTP, no de
        // Infrastructure.
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AdminSeedOptions>(configuration.GetSection(AdminSeedOptions.SectionName));

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<AdminUserSeeder>();

        return services;
    }
}
