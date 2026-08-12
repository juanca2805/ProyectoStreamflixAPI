using Microsoft.EntityFrameworkCore;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Domain.Entities;
using StreamFlix.Infrastructure.Persistence;

namespace StreamFlix.Infrastructure.Repositories;

public class GenreRepository : IGenreRepository
{
    private readonly AppDbContext _context;

    public GenreRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Genre?> GetByIdAsync(Guid id)
    {
        return await _context.Genres.FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<List<Genre>> GetAllAsync()
    {
        return await _context.Genres.AsNoTracking().ToListAsync();
    }

    public async Task<List<Genre>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.Distinct().ToList();
        return await _context.Genres
            .Where(g => idList.Contains(g.Id))
            .ToListAsync();
    }

    public async Task AddAsync(Genre genre)
    {
        await _context.Genres.AddAsync(genre);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
