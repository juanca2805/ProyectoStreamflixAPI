using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StreamFlix.Application.Common.Interfaces;
using StreamFlix.Domain.Entities;
using StreamFlix.Infrastructure.Security;

namespace StreamFlix.Infrastructure.Persistence;

/// <summary>
/// Crea el usuario Admin inicial a partir de "AdminSeed" en la configuración,
/// si todavía no existe uno con ese email. Se ejecuta una vez en cada arranque
/// de la app (Program.cs, junto a las migraciones), y es idempotente: si el
/// admin ya existe, no hace nada.
/// </summary>
public class AdminUserSeeder
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AdminSeedOptions _options;
    private readonly ILogger<AdminUserSeeder> _logger;

    // Se apoya en IUserRepository (no en AppDbContext directo) a propósito: así la
    // normalización del email ("Trim().ToLowerInvariant()") y el AddAsync/SaveChangesAsync
    // quedan en un único lugar (UserRepository), el mismo que usa el login. Si esa regla
    // cambia alguna vez, no hay un segundo lugar donde también haya que acordarse de cambiarla.
    public AdminUserSeeder(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IOptions<AdminSeedOptions> options,
        ILogger<AdminUserSeeder> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (string.IsNullOrWhiteSpace(_options.Email) || string.IsNullOrWhiteSpace(_options.Password))
        {
            _logger.LogWarning(
                "No se configuró 'AdminSeed:Email'/'AdminSeed:Password': no se creará ningún usuario Admin " +
                "y, como todos los endpoints exigen ese rol, la API quedará inaccesible hasta crear uno a mano.");
            return;
        }

        var existing = await _userRepository.GetByEmailAsync(_options.Email);
        if (existing is not null)
            return;

        var passwordHash = _passwordHasher.Hash(_options.Password);
        var admin = new User(_options.Name, _options.Email, passwordHash, UserRole.Admin);

        await _userRepository.AddAsync(admin);

        try
        {
            await _userRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Dos instancias de la app pueden arrancar a la vez apuntando a la misma
            // base (ej. varios tests de integración levantando WebApplicationFactory
            // en paralelo): el check de "existing is null" de arriba no alcanza para
            // evitar la carrera, así que quien pierde la carrera del índice único de
            // Email solo tiene que asumir que la otra instancia ya lo creó.
            _logger.LogInformation(
                "El usuario Admin con email {Email} ya fue creado por otra instancia concurrente.",
                _options.Email);
            return;
        }

        _logger.LogInformation("Usuario Admin inicial creado con email {Email}", admin.Email);
    }
}
