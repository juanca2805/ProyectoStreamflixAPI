using Microsoft.Extensions.Logging;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Application.Movies.Dtos;
using StreamFlix.Domain.Entities;
using StreamFlix.Domain.Exceptions;

namespace StreamFlix.Application.Movies;

/// <summary>
/// Coordina el caso de uso "gestionar películas". No sabe nada de HTTP (eso es de Api)
/// ni de PostgreSQL/EF Core (eso es de Infrastructure): solo conoce las interfaces
/// IMovieRepository / IGenreRepository, que son abstracciones.
///
/// Flujo típico:
///   Controller → MovieService → IMovieRepository → MovieRepository (Infrastructure) → PostgreSQL
/// </summary>
public class MovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly ILogger<MovieService> _logger;

    public MovieService(
        IMovieRepository movieRepository,
        IGenreRepository genreRepository,
        ILogger<MovieService> logger)
    {
        _movieRepository = movieRepository;
        _genreRepository = genreRepository;
        _logger = logger;
    }

    public async Task<List<MovieDto>> GetAllAsync()
    {
        var movies = await _movieRepository.GetAllAsync();
        return movies.Select(MovieDto.FromEntity).ToList();
    }

    public async Task<MovieDto> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Consultando película {MovieId}", id);

        var movie = await _movieRepository.GetByIdAsync(id);
        if (movie is null)
            throw new NotFoundException(nameof(Movie), id);

        return MovieDto.FromEntity(movie);
    }

    public async Task<MovieDto> CreateAsync(CreateMovieRequest request)
    {
        // Validación de negocio adicional (más allá de "campos obligatorios"):
        // los géneros indicados deben existir realmente en la base de datos.
        var genres = await _genreRepository.GetByIdsAsync(request.GenreIds);
        if (genres.Count != request.GenreIds.Distinct().Count())
            throw new ArgumentException("Uno o más géneros indicados no existen.");

        // La validación de "título obligatorio", "duración > 0", etc.
        // ocurre DENTRO del constructor de Movie (Domain), no aquí.
        var movie = new Movie(
            request.Title,
            request.Description,
            request.ReleaseYear,
            request.DurationMinutes,
            request.Rating);

        foreach (var genre in genres)
            movie.AddGenre(genre);

        await _movieRepository.AddAsync(movie);
        await _movieRepository.SaveChangesAsync();

        _logger.LogInformation("Película creada: {MovieId} - {Title}", movie.Id, movie.Title);

        return MovieDto.FromEntity(movie);
    }

    public async Task<MovieDto> UpdateAsync(Guid id, UpdateMovieRequest request)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        if (movie is null)
            throw new NotFoundException(nameof(Movie), id);

        movie.Update(
            request.Title,
            request.Description,
            request.ReleaseYear,
            request.DurationMinutes,
            request.Rating);

        await _movieRepository.SaveChangesAsync();

        return MovieDto.FromEntity(movie);
    }

    public async Task DeleteAsync(Guid id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        if (movie is null)
            throw new NotFoundException(nameof(Movie), id);

        await _movieRepository.DeleteAsync(movie);
        await _movieRepository.SaveChangesAsync();

        _logger.LogInformation("Película eliminada: {MovieId}", id);
    }
}
