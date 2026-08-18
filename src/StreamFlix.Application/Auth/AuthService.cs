using Microsoft.Extensions.Logging;
using StreamFlix.Application.Auth.Dtos;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Application.Users.Dtos;
using StreamFlix.Domain.Exceptions;

namespace StreamFlix.Application.Auth;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        // Mismo mensaje tanto si el email no existe como si la password es
        // incorrecta (ver UnauthorizedException): no hay que darle a quien
        // llama ninguna pista sobre cuál de las dos cosas falló.
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Intento de login fallido para el email {Email}", request.Email);
            throw new UnauthorizedException("Email o contraseña incorrectos.");
        }

        var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);

        _logger.LogInformation("Login exitoso: {UserId}", user.Id);

        return new LoginResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            User = UserDto.FromEntity(user)
        };
    }
}
