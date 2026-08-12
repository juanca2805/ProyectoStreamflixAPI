using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Application.Users;
using StreamFlix.Application.Users.Dtos;
using StreamFlix.Domain.Entities;
using StreamFlix.Domain.Exceptions;
using Xunit;

namespace StreamFlix.UnitTests.Users;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            NullLogger<UserService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_CuandoElEmailYaExiste_LanzaConflictException()
    {
        // Arrange: simulamos que ya hay un usuario con ese email en la "base de datos".
        var existingUser = new User("Otro", "ana@example.com", "hash-existente");
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("ana@example.com"))
            .ReturnsAsync(existingUser);

        var request = new CreateUserRequest
        {
            Name = "Ana",
            Email = "ana@example.com",
            Password = "Secreta123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _sut.CreateAsync(request));

        // No debería intentar hashear ni guardar si el email ya existe.
        _passwordHasherMock.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_CuandoElEmailNoExiste_HasheaLaContraseñaYGuardaElUsuario()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(h => h.Hash("Secreta123"))
            .Returns("hash-simulado");

        var request = new CreateUserRequest
        {
            Name = "Ana",
            Email = "ana@example.com",
            Password = "Secreta123"
        };

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.Equal("ana@example.com", result.Email);
        _passwordHasherMock.Verify(h => h.Hash("Secreta123"), Times.Once);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_CuandoElUsuarioNoExiste_LanzaNotFoundException()
    {
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(Guid.NewGuid()));
    }
}
