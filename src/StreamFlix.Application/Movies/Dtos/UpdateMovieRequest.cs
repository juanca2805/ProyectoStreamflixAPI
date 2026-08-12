namespace StreamFlix.Application.Movies.Dtos;

/// <summary>
/// DTO de entrada para PUT /api/movies/{id}.
/// Es casi idéntico a CreateMovieRequest, pero se mantiene como una clase separada
/// a propósito: "crear" y "actualizar" son casos de uso distintos y pueden divergir
/// con el tiempo (ej. Update podría no permitir cambiar el año en el futuro).
/// </summary>
public class UpdateMovieRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int ReleaseYear { get; init; }
    public int DurationMinutes { get; init; }
    public double Rating { get; init; }
    public List<Guid> GenreIds { get; init; } = new();
}
