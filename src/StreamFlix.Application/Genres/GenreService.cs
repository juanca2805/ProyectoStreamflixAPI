using Microsoft.Extensions.Logging;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Application.Genres.Dtos;
using StreamFlix.Domain.Entities;
using StreamFlix.Domain.Exceptions;

namespace StreamFlix.Application.Genres;

public class GenreService
{
    private readonly IGenreRepository _genreRepository;
    private readonly ILogger<GenreService> _logger;

    public GenreService(IGenreRepository genreRepository, ILogger<GenreService> logger)
    {
        _genreRepository = genreRepository;
        _logger = logger;
    }

    public async Task<List<GenreDto>> GetAllAsync()
    {
        var genres = await _genreRepository.GetAllAsync();
        return genres.Select(GenreDto.FromEntity).ToList();
    }

    public async Task<GenreDto> GetByIdAsync(Guid id)
    {
        var genre = await _genreRepository.GetByIdAsync(id);
        if (genre is null)
            throw new NotFoundException(nameof(Genre), id);

        return GenreDto.FromEntity(genre);
    }

    public async Task<GenreDto> CreateAsync(CreateGenreRequest request)
    {
        var genre = new Genre(request.Name);

        await _genreRepository.AddAsync(genre);
        await _genreRepository.SaveChangesAsync();

        _logger.LogInformation("Género creado: {GenreId} - {Name}", genre.Id, genre.Name);

        return GenreDto.FromEntity(genre);
    }
}
