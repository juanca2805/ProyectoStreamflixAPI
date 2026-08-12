namespace StreamFlix.Domain.Entities;

/// <summary>
/// Registra que un usuario vio (o está viendo) una película, y en qué punto se quedó.
/// Conceptualmente es un registro de TRAZABILIDAD: qué pasó, cuándo y hasta dónde.
/// </summary>
public class WatchHistory
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid MovieId { get; private set; }
    public Movie Movie { get; private set; } = null!;

    public DateTime WatchedAt { get; private set; }

    // Progreso de 0.0 (no iniciado) a 1.0 (visto completo).
    public double Progress { get; private set; }

    private WatchHistory() { }

    public WatchHistory(Guid userId, Guid movieId, double progress)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("El usuario es obligatorio.", nameof(userId));

        if (movieId == Guid.Empty)
            throw new ArgumentException("La película es obligatoria.", nameof(movieId));

        SetProgress(progress);

        Id = Guid.NewGuid();
        UserId = userId;
        MovieId = movieId;
        WatchedAt = DateTime.UtcNow;
    }

    public void UpdateProgress(double progress)
    {
        SetProgress(progress);
        WatchedAt = DateTime.UtcNow;
    }

    private void SetProgress(double progress)
    {
        if (progress < 0 || progress > 1)
            throw new ArgumentException("El progreso debe estar entre 0 y 1.", nameof(progress));

        Progress = progress;
    }
}
