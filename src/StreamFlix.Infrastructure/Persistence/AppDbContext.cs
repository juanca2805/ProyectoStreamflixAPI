using Microsoft.EntityFrameworkCore;
using StreamFlix.Domain.Entities;

namespace StreamFlix.Infrastructure.Persistence;

/// <summary>
/// AppDbContext es el puente entre las entidades de Domain y PostgreSQL.
/// EF Core usa esta clase para:
///   1. Saber qué tablas existen (a través de los DbSet).
///   2. Traducir consultas LINQ (ej. .Where(m => m.Rating > 8)) a SQL.
///   3. Rastrear cambios en memoria (change tracking) y generar los INSERT/UPDATE/DELETE
///      correspondientes cuando se llama a SaveChangesAsync().
///
/// Vive en Infrastructure, NUNCA en Domain ni Application, porque es un detalle
/// 100% técnico: "cómo" se guardan las entidades, no "qué son" ni "qué reglas cumplen".
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();
    public DbSet<User> Users => Set<User>();
    public DbSet<WatchHistory> WatchHistories => Set<WatchHistory>();
    public DbSet<Favorite> Favorites => Set<Favorite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas las clases IEntityTypeConfiguration<T> del ensamblado actual
        // (una por entidad, ver carpeta Persistence/Configurations).
        // Esto evita tener un OnModelCreating gigante con todo mezclado.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
