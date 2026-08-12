using Microsoft.EntityFrameworkCore;
using StreamFlix.Api.Middleware;
using StreamFlix.Application;
using StreamFlix.Infrastructure;
using StreamFlix.Infrastructure.Persistence;

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
builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "StreamFlix API",
        Version = "v1",
        Description = "API educativa para practicar arquitectura limpia en .NET, inspirada conceptualmente en Netflix."
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
app.UseAuthorization();

app.MapControllers();

app.Run();

// Clase parcial vacía: permite que WebApplicationFactory<Program>, usado en
// StreamFlix.IntegrationTests, pueda referenciar el "Program" de esta app de
// nivel superior (top-level statements) para arrancarla en memoria durante los tests.
public partial class Program { }
