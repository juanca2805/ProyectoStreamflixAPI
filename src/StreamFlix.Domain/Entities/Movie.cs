namespace StreamFlix.Domain.Entities;

/// <summary>
/// Representa una película del catálogo.
/// Contiene sus propias reglas de negocio (ej. duración > 0, año válido)
/// para que sea imposible construir una Movie en un estado inválido,
/// sin importar quién la esté creando (API, un test, una migración de datos, etc.).
/// </summary>
public class Movie
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int ReleaseYear { get; private set; }
    public int DurationMinutes { get; private set; }
    public double Rating { get; private set; }

    // Relación muchos-a-muchos con Genre a través de la entidad de asociación MovieGenre.
    // Se modela de forma explícita (en lugar de una lista directa de Genre) porque
    // EF Core moderno soporta muchos-a-muchos implícito, pero mantenerlo explícito
    // en el dominio deja ver claramente la relación sin "magia" oculta.
    private readonly List<MovieGenre> _movieGenres = new();
    public IReadOnlyCollection<MovieGenre> MovieGenres => _movieGenres.AsReadOnly();

    private Movie() { }

    public Movie(string title, string description, int releaseYear, int durationMinutes, double rating)
    {
        SetTitle(title);
        SetDescription(description);
        SetReleaseYear(releaseYear);
        SetDuration(durationMinutes);
        SetRating(rating);

        Id = Guid.NewGuid();
    }

    public void Update(string title, string description, int releaseYear, int durationMinutes, double rating)
    {
        SetTitle(title);
        SetDescription(description);
        SetReleaseYear(releaseYear);
        SetDuration(durationMinutes);
        SetRating(rating);
    }

    public void AddGenre(Genre genre)
    {
        if (genre is null)
            throw new ArgumentNullException(nameof(genre));

        if (_movieGenres.Any(mg => mg.GenreId == genre.Id))
            return; // ya asociado, evita duplicados

        _movieGenres.Add(new MovieGenre(this, genre));
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título de la película es obligatorio.", nameof(title));

        Title = title.Trim();
    }

    private void SetDescription(string description)
    {
        Description = description?.Trim() ?? string.Empty;
    }

    private void SetReleaseYear(int releaseYear)
    {
        var minYear = 1888; // año de la primera película registrada en la historia
        var maxYear = DateTime.UtcNow.Year + 1; // permite anunciar próximos estrenos

        if (releaseYear < minYear || releaseYear > maxYear)
            throw new ArgumentException($"El año de estreno debe estar entre {minYear} y {maxYear}.", nameof(releaseYear));

        ReleaseYear = releaseYear;
    }

    private void SetDuration(int durationMinutes)
    {
        if (durationMinutes <= 0)
            throw new ArgumentException("La duración debe ser mayor a 0 minutos.", nameof(durationMinutes));

        DurationMinutes = durationMinutes;
    }

    private void SetRating(double rating)
    {
        if (rating < 0 || rating > 10)
            throw new ArgumentException("El rating debe estar entre 0 y 10.", nameof(rating));

        Rating = rating;
    }
}
