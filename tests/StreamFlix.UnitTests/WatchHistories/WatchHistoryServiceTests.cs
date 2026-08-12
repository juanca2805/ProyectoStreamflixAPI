using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Application.WatchHistories;
using StreamFlix.Application.WatchHistories.Dtos;
using StreamFlix.Domain.Entities;
using StreamFlix.Domain.Exceptions;
using Xunit;

namespace StreamFlix.UnitTests.WatchHistories;

public class WatchHistoryServiceTests
{
    private readonly Mock<IWatchHistoryRepository> _watchHistoryRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IMovieRepository> _movieRepositoryMock = new();
    private readonly WatchHistoryService _sut;

    public WatchHistoryServiceTests()
    {
        _sut = new WatchHistoryService(
            _watchHistoryRepositoryMock.Object,
            _userRepositoryMock.Object,
            _movieRepositoryMock.Object,
            NullLogger<WatchHistoryService>.Instance);
    }

    [Fact]
    public async Task AddAsync_CuandoLaPeliculaNoExiste_LanzaNotFoundException()
    {
        var userId = Guid.NewGuid();

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new User("Ana", "ana@example.com", "hash"));

        _movieRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Movie?)null);

        var request = new CreateWatchHistoryRequest { MovieId = Guid.NewGuid(), Progress = 0.5 };

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.AddAsync(userId, request));
    }

    [Fact]
    public async Task AddAsync_CuandoTodoEsValido_RegistraElHistorial()
    {
        var userId = Guid.NewGuid();
        var movie = new Movie("Una película", "desc", 2020, 100, 7);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new User("Ana", "ana@example.com", "hash"));

        _movieRepositoryMock.Setup(r => r.GetByIdAsync(movie.Id))
            .ReturnsAsync(movie);

        var request = new CreateWatchHistoryRequest { MovieId = movie.Id, Progress = 0.75 };

        var result = await _sut.AddAsync(userId, request);

        Assert.Equal(0.75, result.Progress);
        _watchHistoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<WatchHistory>()), Times.Once);
    }
}
