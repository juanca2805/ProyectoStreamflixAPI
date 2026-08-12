using StreamFlix.Domain.Entities;

namespace StreamFlix.Application.Favorites.Dtos;

public class FavoriteDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid MovieId { get; init; }
    public string MovieTitle { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }

    public static FavoriteDto FromEntity(Favorite favorite) => new()
    {
        Id = favorite.Id,
        UserId = favorite.UserId,
        MovieId = favorite.MovieId,
        MovieTitle = favorite.Movie?.Title ?? string.Empty,
        CreatedAt = favorite.CreatedAt
    };
}
