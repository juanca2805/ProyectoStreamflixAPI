using Microsoft.Extensions.Logging;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Application.Users.Dtos;
using StreamFlix.Domain.Entities;
using StreamFlix.Domain.Exceptions;

namespace StreamFlix.Application.Users;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException(nameof(User), id);

        return UserDto.FromEntity(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        // Validación de negocio: el email debe ser único.
        // Esto NO puede validarse dentro de la entidad User (Domain), porque
        // requiere consultar la base de datos, algo que Domain no puede hacer.
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
            throw new ConflictException($"Ya existe un usuario con el email '{request.Email}'.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User(request.Name, request.Email, passwordHash);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Usuario creado: {UserId}", user.Id);

        return UserDto.FromEntity(user);
    }
}
