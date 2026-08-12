using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StreamFlix.Domain.Entities;

namespace StreamFlix.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API: aquí se le dice a EF Core detalles de mapeo que no puede adivinar solo,
/// por ejemplo longitudes máximas de columnas o nombres de tabla.
/// Se usa Fluent API (en vez de Data Annotations como [Required] en la entidad)
/// para mantener Domain totalmente libre de atributos de EF Core.
/// </summary>
public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("Movies");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .HasMaxLength(2000);

        builder.Property(m => m.Rating)
            .HasColumnType("double precision");

        // El campo _movieGenres es privado en Movie; le decimos a EF Core
        // que acceda a él mediante el backing field, no mediante una propiedad pública.
        builder.Metadata
            .FindNavigation(nameof(Movie.MovieGenres))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
