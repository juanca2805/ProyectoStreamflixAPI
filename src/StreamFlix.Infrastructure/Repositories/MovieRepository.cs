using Microsoft.EntityFrameworkCore;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Domain.Entities;
using StreamFlix.Infrastructure.Persistence;

namespace StreamFlix.Infrastructure.Repositories;

/// <summary>
/// Implementación concreta de IMovieRepository usando EF Core + PostgreSQL.
/// Esta es la ÚNICA clase del proyecto que sabe que "guardar una película"
/// significa ejecutar SQL contra una tabla Movies en PostgreSQL.
/// </summary>
public class MovieRepository : IMovieRepository
{
    private readonly AppDbContext _context;

    public MovieRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        return await _context.Movies
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<Movie>> GetAllAsync()
    {
        return await _context.Movies
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Movie movie)
    {
        await _context.Movies.AddAsync(movie);
    }

    public Task DeleteAsync(Movie movie)
    {
        _context.Movies.Remove(movie);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
