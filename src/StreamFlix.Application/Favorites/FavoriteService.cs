using Microsoft.Extensions.Logging;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Application.Favorites.Dtos;
using StreamFlix.Domain.Entities;
using StreamFlix.Domain.Exceptions;

namespace StreamFlix.Application.Favorites;

public class FavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(
        IFavoriteRepository favoriteRepository,
        IUserRepository userRepository,
        IMovieRepository movieRepository,
        ILogger<FavoriteService> logger)
    {
        _favoriteRepository = favoriteRepository;
        _userRepository = userRepository;
        _movieRepository = movieRepository;
        _logger = logger;
    }

    public async Task<List<FavoriteDto>> GetByUserIdAsync(Guid userId)
    {
        var favorites = await _favoriteRepository.GetByUserIdAsync(userId);
        return favorites.Select(FavoriteDto.FromEntity).ToList();
    }

    public async Task<FavoriteDto> AddAsync(Guid userId, Guid movieId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException(nameof(User), userId);

        var movie = await _movieRepository.GetByIdAsync(movieId);
        if (movie is null)
            throw new NotFoundException(nameof(Movie), movieId);

        var existing = await _favoriteRepository.GetByUserAndMovieAsync(userId, movieId);
        if (existing is not null)
            throw new ConflictException("La película ya está en favoritos.");

        var favorite = new Favorite(userId, movieId);
        await _favoriteRepository.AddAsync(favorite);
        await _favoriteRepository.SaveChangesAsync();

        _logger.LogInformation("Favorito agregado: usuario {UserId}, película {MovieId}", userId, movieId);

        return FavoriteDto.FromEntity(favorite);
    }

    public async Task RemoveAsync(Guid userId, Guid movieId)
    {
        var favorite = await _favoriteRepository.GetByUserAndMovieAsync(userId, movieId);
        if (favorite is null)
            throw new NotFoundException(nameof(Favorite), $"{userId}/{movieId}");

        await _favoriteRepository.DeleteAsync(favorite);
        await _favoriteRepository.SaveChangesAsync();
    }
}
