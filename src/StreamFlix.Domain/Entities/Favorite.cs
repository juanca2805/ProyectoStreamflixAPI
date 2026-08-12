namespace StreamFlix.Domain.Entities;

/// <summary>
/// Representa que un usuario marcó una película como favorita.
/// Es intencionalmente una entidad muy simple: solo une un User con una Movie.
/// </summary>
public class Favorite
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid MovieId { get; private set; }
    public Movie Movie { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    private Favorite() { }

    public Favorite(Guid userId, Guid movieId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("El usuario es obligatorio.", nameof(userId));

        if (movieId == Guid.Empty)
            throw new ArgumentException("La película es obligatoria.", nameof(movieId));

        Id = Guid.NewGuid();
        UserId = userId;
        MovieId = movieId;
        CreatedAt = DateTime.UtcNow;
    }
}
