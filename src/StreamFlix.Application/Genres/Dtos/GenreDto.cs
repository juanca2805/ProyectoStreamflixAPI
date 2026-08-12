using StreamFlix.Domain.Entities;

namespace StreamFlix.Application.Genres.Dtos;

public class GenreDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;

    public static GenreDto FromEntity(Genre genre) => new()
    {
        Id = genre.Id,
        Name = genre.Name
    };
}
