namespace StreamFlix.Application.Movies.Dtos;

/// <summary>
/// DTO de ENTRADA (Request DTO): lo que el cliente envía en el body de POST /api/movies.
/// Solo contiene los campos que el cliente puede/debe enviar (nunca un Id, por ejemplo:
/// eso lo genera el servidor).
/// </summary>
public class CreateMovieRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int ReleaseYear { get; init; }
    public int DurationMinutes { get; init; }
    public double Rating { get; init; }
    public List<Guid> GenreIds { get; init; } = new();
}
