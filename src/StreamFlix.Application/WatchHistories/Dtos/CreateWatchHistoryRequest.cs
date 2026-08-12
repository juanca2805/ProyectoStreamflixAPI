namespace StreamFlix.Application.WatchHistories.Dtos;

/// <summary>
/// DTO de entrada para POST /api/users/{userId}/history.
/// UserId no viaja aquí: llega por la ruta (route parameter), no por el body,
/// porque conceptualmente "de quién es este historial" es parte de la URL, no del payload.
/// </summary>
public class CreateWatchHistoryRequest
{
    public Guid MovieId { get; init; }
    public double Progress { get; init; }
}
