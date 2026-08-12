using StreamFlix.Domain.Entities;

namespace StreamFlix.Application.Movies.Dtos;

/// <summary>
/// DTO de RESPUESTA: lo que la API devuelve al cliente cuando consulta una película.
/// No es lo mismo que la entidad Movie: aquí decidimos exactamente qué campos
/// se exponen (por ejemplo, "aplanamos" los géneros a una lista de nombres,
/// en lugar de exponer la colección interna de MovieGenre).
/// </summary>
public class MovieDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int ReleaseYear { get; init; }
    public int DurationMinutes { get; init; }
    public double Rating { get; init; }
    public List<string> Genres { get; init; } = new();

    public static MovieDto FromEntity(Movie movie)
    {
        return new MovieDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseYear = movie.ReleaseYear,
            DurationMinutes = movie.DurationMinutes,
            Rating = movie.Rating,
            Genres = movie.MovieGenres.Select(mg => mg.Genre.Name).ToList()
        };
    }
}
