using Microsoft.EntityFrameworkCore;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Domain.Entities;
using StreamFlix.Infrastructure.Persistence;

namespace StreamFlix.Infrastructure.Repositories;

public class WatchHistoryRepository : IWatchHistoryRepository
{
    private readonly AppDbContext _context;

    public WatchHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<WatchHistory>> GetByUserIdAsync(Guid userId)
    {
        return await _context.WatchHistories
            .Include(w => w.Movie)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.WatchedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(WatchHistory watchHistory)
    {
        await _context.WatchHistories.AddAsync(watchHistory);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
