using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StreamFlix.Domain.Entities;

namespace StreamFlix.Infrastructure.Persistence.Configurations;

public class WatchHistoryConfiguration : IEntityTypeConfiguration<WatchHistory>
{
    public void Configure(EntityTypeBuilder<WatchHistory> builder)
    {
        builder.ToTable("WatchHistories");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Progress)
            .HasColumnType("double precision");

        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Movie)
            .WithMany()
            .HasForeignKey(w => w.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
