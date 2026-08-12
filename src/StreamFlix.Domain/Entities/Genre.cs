namespace StreamFlix.Domain.Entities;

/// <summary>
/// Representa un género de película (ej. "Acción", "Comedia").
/// Es una entidad simple, usada principalmente como catálogo
/// y como parte de la relación muchos-a-muchos con Movie.
/// </summary>
public class Genre
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    // Lado inverso de la relación muchos-a-muchos con Movie, a través de la
    // entidad de asociación MovieGenre (ver Movie.MovieGenres). Se modela así,
    // en lugar de una List<Movie> directa, para que ambos lados de la relación
    // sean simétricos y EF Core pueda mapear el join sin ambigüedad.
    private readonly List<MovieGenre> _movieGenres = new();
    public IReadOnlyCollection<MovieGenre> MovieGenres => _movieGenres.AsReadOnly();

    // Constructor privado sin parámetros: lo exige Entity Framework Core
    // para poder materializar (reconstruir) la entidad al leerla de la base de datos,
    // sin obligarnos a exponer un constructor público sin validaciones.
    private Genre() { }

    public Genre(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del género es obligatorio.", nameof(name));

        Id = Guid.NewGuid();
        Name = name.Trim();
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("El nombre del género es obligatorio.", nameof(newName));

        Name = newName.Trim();
    }
}
