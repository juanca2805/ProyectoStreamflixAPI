using Microsoft.Extensions.Logging;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Application.WatchHistories.Dtos;
using StreamFlix.Domain.Entities;
using StreamFlix.Domain.Exceptions;

namespace StreamFlix.Application.WatchHistories;

public class WatchHistoryService
{
    private readonly IWatchHistoryRepository _watchHistoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly ILogger<WatchHistoryService> _logger;

    public WatchHistoryService(
        IWatchHistoryRepository watchHistoryRepository,
        IUserRepository userRepository,
        IMovieRepository movieRepository,
        ILogger<WatchHistoryService> logger)
    {
        _watchHistoryRepository = watchHistoryRepository;
        _userRepository = userRepository;
        _movieRepository = movieRepository;
        _logger = logger;
    }

    public async Task<List<WatchHistoryDto>> GetByUserIdAsync(Guid userId)
    {
        var history = await _watchHistoryRepository.GetByUserIdAsync(userId);
        return history.Select(WatchHistoryDto.FromEntity).ToList();
    }

    public async Task<WatchHistoryDto> AddAsync(Guid userId, CreateWatchHistoryRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException(nameof(User), userId);

        var movie = await _movieRepository.GetByIdAsync(request.MovieId);
        if (movie is null)
            throw new NotFoundException(nameof(Movie), request.MovieId);

        var watchHistory = new WatchHistory(userId, request.MovieId, request.Progress);

        await _watchHistoryRepository.AddAsync(watchHistory);
        await _watchHistoryRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Historial registrado: usuario {UserId}, película {MovieId}, progreso {Progress}",
            userId, request.MovieId, request.Progress);

        return WatchHistoryDto.FromEntity(watchHistory);
    }
}
