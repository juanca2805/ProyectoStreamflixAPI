using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Application.Favorites;
using StreamFlix.Domain.Entities;
using StreamFlix.Domain.Exceptions;
using Xunit;

namespace StreamFlix.UnitTests.Favorites;

public class FavoriteServiceTests
{
    private readonly Mock<IFavoriteRepository> _favoriteRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IMovieRepository> _movieRepositoryMock = new();
    private readonly FavoriteService _sut;

    public FavoriteServiceTests()
    {
        _sut = new FavoriteService(
            _favoriteRepositoryMock.Object,
            _userRepositoryMock.Object,
            _movieRepositoryMock.Object,
            NullLogger<FavoriteService>.Instance);
    }

    [Fact]
    public async Task AddAsync_CuandoElUsuarioNoExiste_LanzaNotFoundException()
    {
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.AddAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task AddAsync_CuandoLaPeliculaYaEsFavorita_LanzaConflictException()
    {
        var userId = Guid.NewGuid();
        var movieId = Guid.NewGuid();

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new User("Ana", "ana@example.com", "hash"));

        _movieRepositoryMock.Setup(r => r.GetByIdAsync(movieId))
            .ReturnsAsync(new Movie("Una película", "desc", 2020, 100, 7));

        _favoriteRepositoryMock
            .Setup(r => r.GetByUserAndMovieAsync(userId, movieId))
            .ReturnsAsync(new Favorite(userId, movieId));

        await Assert.ThrowsAsync<ConflictException>(() => _sut.AddAsync(userId, movieId));
    }

    [Fact]
    public async Task AddAsync_CuandoTodoEsValido_AgregaElFavorito()
    {
        var userId = Guid.NewGuid();
        var movieId = Guid.NewGuid();

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new User("Ana", "ana@example.com", "hash"));

        _movieRepositoryMock.Setup(r => r.GetByIdAsync(movieId))
            .ReturnsAsync(new Movie("Una película", "desc", 2020, 100, 7));

        _favoriteRepositoryMock
            .Setup(r => r.GetByUserAndMovieAsync(userId, movieId))
            .ReturnsAsync((Favorite?)null);

        var result = await _sut.AddAsync(userId, movieId);

        Assert.Equal(userId, result.UserId);
        Assert.Equal(movieId, result.MovieId);
        _favoriteRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Favorite>()), Times.Once);
    }
}
