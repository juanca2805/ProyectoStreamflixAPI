using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StreamFlix.Application.Auth;
using StreamFlix.Application.Auth.Dtos;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Domain.Entities;
using StreamFlix.Domain.Exceptions;
using Xunit;

namespace StreamFlix.UnitTests.Auth;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task LoginAsync_CuandoElEmailNoExiste_LanzaUnauthorizedException()
    {
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var request = new LoginRequest { Email = "nadie@example.com", Password = "Cualquiera123" };

        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(request));

        _jwtTokenGeneratorMock.Verify(g => g.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_CuandoLaPasswordEsIncorrecta_LanzaUnauthorizedException()
    {
        var user = new User("Ana", "ana@example.com", "hash-almacenado", UserRole.Admin);
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("ana@example.com"))
            .ReturnsAsync(user);
        _passwordHasherMock
            .Setup(h => h.Verify("incorrecta", "hash-almacenado"))
            .Returns(false);

        var request = new LoginRequest { Email = "ana@example.com", Password = "incorrecta" };

        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(request));

        _jwtTokenGeneratorMock.Verify(g => g.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_CuandoLasCredencialesSonValidas_DevuelveElTokenGenerado()
    {
        var user = new User("Ana", "ana@example.com", "hash-almacenado", UserRole.Admin);
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("ana@example.com"))
            .ReturnsAsync(user);
        _passwordHasherMock
            .Setup(h => h.Verify("Secreta123", "hash-almacenado"))
            .Returns(true);

        var expiresAtUtc = DateTime.UtcNow.AddHours(1);
        _jwtTokenGeneratorMock
            .Setup(g => g.GenerateToken(user))
            .Returns(("token-simulado", expiresAtUtc));

        var request = new LoginRequest { Email = "ana@example.com", Password = "Secreta123" };

        var result = await _sut.LoginAsync(request);

        Assert.Equal("token-simulado", result.Token);
        Assert.Equal(expiresAtUtc, result.ExpiresAtUtc);
        Assert.Equal(user.Email, result.User.Email);
    }
}
