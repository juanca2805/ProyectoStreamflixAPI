using Microsoft.Extensions.Logging;
using Moq;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Application.Movies;
using StreamFlix.Application.Movies.Dtos;
using StreamFlix.Domain.Entities;
using StreamFlix.Domain.Exceptions;
using Xunit;

namespace StreamFlix.UnitTests.Movies;

/// <summary>
/// Estos tests NO usan PostgreSQL. En vez de eso, se reemplaza IMovieRepository
/// por un Mock (un objeto falso que simula respuestas predefinidas).
///
///     MovieService
///           │
///           ▼
///     IMovieRepository
///           │
///           └── Mock  ←── aquí decidimos exactamente qué devuelve, sin base de datos
///
/// Lo que se prueba realmente es la LÓGICA de MovieService: qué hace con lo que
/// el repositorio le devuelve (o no le devuelve), no si Postgres funciona.
/// </summary>
public class MovieServiceTests
{
    private readonly Mock<IMovieRepository> _movieRepositoryMock = new();
    private readonly Mock<IGenreRepository> _genreRepositoryMock = new();
    private readonly MovieService _sut; // "sut" = System Under Test

    public MovieServiceTests()
    {
        _sut = new MovieService(
            _movieRepositoryMock.Object,
            _genreRepositoryMock.Object,
            NullLogger());
    }

    [Fact]
    public async Task GetByIdAsync_CuandoLaPeliculaExiste_DevuelveElDto()
    {
        // Arrange
        var movie = new Movie("Interstellar", "Viaje espacial", 2014, 169, 8.6);
        _movieRepositoryMock
            .Setup(r => r.GetByIdAsync(movie.Id))
            .ReturnsAsync(movie);

        // Act
        var result = await _sut.GetByIdAsync(movie.Id);

        // Assert
        Assert.Equal(movie.Id, result.Id);
        Assert.Equal("Interstellar", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_CuandoLaPeliculaNoExiste_LanzaNotFoundException()
    {
        // Arrange: el mock devuelve null, como haría el repositorio real si no encuentra el id.
        _movieRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Movie?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_CuandoUnGeneroNoExiste_LanzaArgumentException()
    {
        // Arrange: se piden 2 géneros pero el repositorio solo "encuentra" 1.
        var existingGenreId = Guid.NewGuid();
        var missingGenreId = Guid.NewGuid();

        _genreRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Genre> { new("Drama") });

        var request = new CreateMovieRequest
        {
            Title = "Una película",
            Description = "Descripción",
            ReleaseYear = 2020,
            DurationMinutes = 100,
            Rating = 7,
            GenreIds = new List<Guid> { existingGenreId, missingGenreId }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(request));

        // El repositorio de películas NUNCA debería haber sido llamado si la validación falla antes.
        _movieRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Movie>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_CuandoLosDatosSonValidos_GuardaLaPeliculaYDevuelveElDto()
    {
        // Arrange
        _genreRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<Genre>());

        var request = new CreateMovieRequest
        {
            Title = "Una película",
            Description = "Descripción",
            ReleaseYear = 2020,
            DurationMinutes = 100,
            Rating = 7,
            GenreIds = new List<Guid>()
        };

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.Equal("Una película", result.Title);
        _movieRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Movie>()), Times.Once);
        _movieRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CuandoLaPeliculaNoExiste_LanzaNotFoundException()
    {
        _movieRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Movie?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid()));

        _movieRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Movie>()), Times.Never);
    }

    private static ILogger<MovieService> NullLogger() =>
        Microsoft.Extensions.Logging.Abstractions.NullLogger<MovieService>.Instance;
}
