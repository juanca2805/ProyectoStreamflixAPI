namespace StreamFlix.Domain.Entities;

/// <summary>
/// Entidad de asociación (tabla intermedia) para la relación muchos-a-muchos
/// entre Movie y Genre. Una película puede tener varios géneros,
/// y un género puede pertenecer a varias películas.
/// </summary>
public class MovieGenre
{
    public Guid MovieId { get; private set; }
    public Movie Movie { get; private set; } = null!;

    public Guid GenreId { get; private set; }
    public Genre Genre { get; private set; } = null!;

    private MovieGenre() { }

    internal MovieGenre(Movie movie, Genre genre)
    {
        Movie = movie ?? throw new ArgumentNullException(nameof(movie));
        MovieId = movie.Id;

        Genre = genre ?? throw new ArgumentNullException(nameof(genre));
        GenreId = genre.Id;
    }
}
