using StreamFlix.Domain.Entities;

namespace StreamFlix.Application.WatchHistories.Dtos;

public class WatchHistoryDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid MovieId { get; init; }
    public string MovieTitle { get; init; } = string.Empty;
    public DateTime WatchedAt { get; init; }
    public double Progress { get; init; }

    public static WatchHistoryDto FromEntity(WatchHistory history) => new()
    {
        Id = history.Id,
        UserId = history.UserId,
        MovieId = history.MovieId,
        MovieTitle = history.Movie?.Title ?? string.Empty,
        WatchedAt = history.WatchedAt,
        Progress = history.Progress
    };
}
