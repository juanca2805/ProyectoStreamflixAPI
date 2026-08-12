using StreamFlix.Domain.Entities;

namespace StreamFlix.Application.Common.Interfaces;

public interface IWatchHistoryRepository
{
    Task<List<WatchHistory>> GetByUserIdAsync(Guid userId);
    Task AddAsync(WatchHistory watchHistory);
    Task SaveChangesAsync();
}
