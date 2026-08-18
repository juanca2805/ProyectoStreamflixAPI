using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using StreamFlix.Api.Middleware;
using StreamFlix.Application;
using StreamFlix.Domain.Entities;
using StreamFlix.Infrastructure;
using StreamFlix.Infrastructure.Persistence;
using StreamFlix.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// --- Inyección de dependencias ---
//
// ¿Quién solicita la dependencia?   Los Controllers (ej. MoviesController pide un MovieService).
// ¿Quién la implementa?             MovieService (Application) y MovieRepository (Infrastructure).
// ¿Quién la crea?                   El contenedor de DI de ASP.NET Core, aquí, en Program.cs.
// ¿Por qué no `new MovieRepository()` dentro del Controller?
//   Porque el Controller tendría que conocer EF Core, la cadena de conexión y
//   cómo construir un AppDbContext. Con DI, el Controller solo declara "necesito
//   un MovieService" en su constructor, y el framework se lo entrega ya armado.
//   Esto también es lo que permite reemplazar MovieRepository por un mock en los tests
//   sin tocar ni una línea del Controller.
//
// AuthorizeFilter global: por defecto, CADA acción de CADA controller exige
// un usuario autenticado que cumpla la policy "AdminOnly" (ver más abajo),
// sin tener que decorar cada controller con [Authorize] a mano y sin
// arriesgarse a olvidarlo en uno nuevo. AuthController es la única excepción,
// marcada explícitamente con [AllowAnonymous]: sin login no hay token, y sin
// token ningún otro endpoint sería alcanzable.
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter("AdminOnly"));
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// --- Autenticación y autorización JWT ---
//
// AddJwtBearer valida los tokens que llegan en el header "Authorization: Bearer {token}".
// Se bindea el mismo JwtOptions que usa JwtTokenGenerator (Infrastructure) para firmarlos
// -en vez de releer "Jwt:Key/Issuer/Audience" con indexers sueltos- para que un cambio de
// nombre de sección o de propiedad rompa la compilación en los dos lados a la vez, no
// silenciosamente en uno solo.
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException($"No se encontró la sección '{JwtOptions.SectionName}' en la configuración.");

if (string.IsNullOrEmpty(jwtOptions.Key))
    throw new InvalidOperationException($"No se encontró la configuración '{JwtOptions.SectionName}:Key'.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    // UserRole.Admin.ToString() (no el literal "Admin" suelto): así un rename del enum
    // rompe esta línea en la compilación en vez de dejar a todo el mundo con 403 en silencio.
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRole.Admin.ToString()));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "StreamFlix API",
        Version = "v1",
        Description = "API educativa para practicar arquitectura limpia en .NET, inspirada conceptualmente en Netflix."
    });

    // Agrega el botón "Authorize" en Swagger UI para poder pegar el JWT
    // (obtenido en POST /api/auth/login) y que viaje en cada request de prueba.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegar solo el token (sin el prefijo \"Bearer \"), obtenido en POST /api/auth/login."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
    });
});

var app = builder.Build();

// --- Migraciones automáticas al arrancar ---
//
// Sin esto, alguien que clona el repo y corre `docker compose up` se
// encuentra con una base de datos sin tablas: el esquema solo se crea si
// alguien ejecuta `dotnet ef database update` a mano, y quien recién clona
// el repo no tiene por qué saber ni tener instalado `dotnet-ef`.
//
// Database.Migrate() aplica las migraciones pendientes (o no hace nada si
// ya están todas aplicadas), así que es seguro dejarlo correr en cada
// arranque, tanto en Docker como en local.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Idempotente: si el usuario configurado en "AdminSeed" ya existe, no hace nada.
    // Sin esto, con el filtro global "AdminOnly" ya activo, nadie podría loguearse
    // nunca para conseguir el primer token.
    var adminSeeder = scope.ServiceProvider.GetRequiredService<AdminUserSeeder>();
    await adminSeeder.SeedAsync();
}

// --- Pipeline HTTP ---
//
// UseExceptionHandling va primero (envuelve a todo lo demás) para poder
// capturar excepciones lanzadas por cualquier middleware o Controller posterior.
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "StreamFlix API v1");
    });
}

// No se usa UseHttpsRedirection(): el perfil "http" de launchSettings.json
// solo expone una URL HTTP (a propósito, para mantener el proyecto simple
// sin gestionar certificados de desarrollo). Si más adelante se sirve detrás
// de HTTPS (ej. un proxy en producción), se puede volver a agregar.
//
// UseAuthentication ANTES que UseAuthorization: primero hay que resolver
// "quién es" (leer y validar el JWT del header, si vino uno) antes de poder
// decidir "qué puede hacer" (evaluar la policy "AdminOnly").
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Clase parcial vacía: permite que WebApplicationFactory<Program>, usado en
// StreamFlix.IntegrationTests, pueda referenciar el "Program" de esta app de
// nivel superior (top-level statements) para arrancarla en memoria durante los tests.
public partial class Program { }
