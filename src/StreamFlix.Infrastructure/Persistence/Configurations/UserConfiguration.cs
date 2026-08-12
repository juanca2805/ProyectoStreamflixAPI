using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StreamFlix.Domain.Entities;

namespace StreamFlix.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(320); // longitud máxima estándar para un email (RFC 5321)

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        // Un email no puede repetirse: se refuerza también a nivel de base de datos,
        // no solo en UserService, por si en el futuro otro proceso escribe directo en la tabla.
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
