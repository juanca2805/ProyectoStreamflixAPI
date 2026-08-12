using Microsoft.EntityFrameworkCore;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Domain.Entities;
using StreamFlix.Infrastructure.Persistence;

namespace StreamFlix.Infrastructure.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly AppDbContext _context;

    public FavoriteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Favorite>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Favorites
            .Include(f => f.Movie)
            .Where(f => f.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Favorite?> GetByUserAndMovieAsync(Guid userId, Guid movieId)
    {
        return await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.MovieId == movieId);
    }

    public async Task AddAsync(Favorite favorite)
    {
        await _context.Favorites.AddAsync(favorite);
    }

    public Task DeleteAsync(Favorite favorite)
    {
        _context.Favorites.Remove(favorite);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
