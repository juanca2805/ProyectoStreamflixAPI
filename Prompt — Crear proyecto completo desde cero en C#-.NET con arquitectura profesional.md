# Prompt — Crear proyecto completo desde cero en C#/.NET con arquitectura profesional

Quiero que construyas desde cero un proyecto backend educativo llamado **StreamFlix**, inspirado conceptualmente en una plataforma como Netflix.

## OBJETIVO PRINCIPAL

El objetivo NO es crear un clon visual de Netflix.

Quiero crear un proyecto backend completo para **aprender y practicar una arquitectura profesional en C#/.NET**, aplicando una estructura similar a la que podría utilizarse posteriormente en un proyecto empresarial como DDL-LIMA.

Quiero que el proyecto tenga:

- API REST.
- C#/.NET.
- PostgreSQL.
- Docker.
- Entity Framework Core.
- Migraciones.
- DTOs.
- Servicios de aplicación.
- Dominio.
- Repositorios.
- Inyección de dependencias.
- Manejo de errores.
- Validaciones.
- Tests.
- Swagger/OpenAPI.
- Configuración por ambientes.
- Una arquitectura limpia y fácil de entender.

El proyecto debe ser **funcional**, pero debe priorizarse la claridad de la arquitectura sobre crear muchas funcionalidades.

---

# 1. Stack

Utiliza:

```text
C#
.NET
ASP.NET Core Web API
PostgreSQL
Docker
Entity Framework Core
Swagger / OpenAPI
xUnit
```

Para PostgreSQL utiliza Docker Compose.

No necesito frontend.

---

# 2. Arquitectura

Utiliza una arquitectura por capas inspirada en Clean Architecture / arquitectura orientada al dominio.

La estructura principal debe ser:

```text
StreamFlix/
│
├── src/
│   │
│   ├── StreamFlix.Api/
│   │
│   ├── StreamFlix.Application/
│   │
│   ├── StreamFlix.Domain/
│   │
│   └── StreamFlix.Infrastructure/
│
├── tests/
│   │
│   ├── StreamFlix.UnitTests/
│   └── StreamFlix.IntegrationTests/
│
├── docker-compose.yml
├── README.md
└── StreamFlix.sln
```

La dependencia conceptual debe ser:

```text
                    ┌──────────────┐
                    │    Client    │
                    └──────┬───────┘
                           │
                           ▼
                    ┌──────────────┐
                    │     API      │
                    │ Controllers  │
                    └──────┬───────┘
                           │
                           ▼
                  ┌──────────────────┐
                  │   Application    │
                  │                  │
                  │ Use Cases        │
                  │ Services         │
                  │ DTOs             │
                  │ Interfaces       │
                  └────────┬─────────┘
                           │
                           ▼
                  ┌──────────────────┐
                  │      Domain      │
                  │                  │
                  │ Entities         │
                  │ Value Objects    │
                  │ Business Rules   │
                  └────────┬─────────┘
                           │
                           ▼
                ┌─────────────────────┐
                │   Infrastructure    │
                │                     │
                │ EF Core             │
                │ PostgreSQL          │
                │ Repositories        │
                │ External Services   │
                └─────────────────────┘
```

---

# 3. MUY IMPORTANTE: explica la arquitectura

No quiero que simplemente generes todos los archivos.

Quiero que expliques **por qué existe cada capa**.

Para cada proyecto:

```text
StreamFlix.Api
StreamFlix.Application
StreamFlix.Domain
StreamFlix.Infrastructure
StreamFlix.UnitTests
StreamFlix.IntegrationTests
```

explica:

- Qué responsabilidad tiene.
- Qué puede conocer.
- Qué NO debería conocer.
- Qué proyectos puede utilizar.
- Qué proyectos no debería utilizar.
- Por qué esta separación es útil.

---

# 4. Dominio

Crea como mínimo las siguientes entidades:

```text
Movie
Genre
User
WatchHistory
Favorite
```

Puedes simplificarlas para mantener el proyecto entendible.

Por ejemplo:

```text
Movie
├── Id
├── Title
├── Description
├── ReleaseYear
├── Duration
├── Rating
└── Genres

User
├── Id
├── Name
├── Email
└── PasswordHash

Genre
├── Id
└── Name

WatchHistory
├── Id
├── UserId
├── MovieId
├── WatchedAt
└── Progress

Favorite
├── Id
├── UserId
└── MovieId
```

Explica cada entidad y sus relaciones.

---

# 5. Base de datos

Utiliza:

```text
PostgreSQL
```

y crea la base de datos mediante:

```text
docker-compose.yml
```

Ejemplo conceptual:

```text
StreamFlix API
      │
      │
      ▼
PostgreSQL
      │
      ├── Users
      ├── Movies
      ├── Genres
      ├── WatchHistory
      └── Favorites
```

La aplicación debe conectarse a PostgreSQL mediante Entity Framework Core.

---

# 6. Docker

Crea:

```text
docker-compose.yml
```

que levante PostgreSQL.

Incluye:

- imagen de PostgreSQL;
- usuario;
- contraseña;
- base de datos;
- puerto;
- volumen para persistencia.

Explica cada configuración.

No necesitas dockerizar la API inicialmente.

La prioridad es que PostgreSQL funcione mediante Docker.

---

# 7. Entity Framework Core

Crea:

```text
AppDbContext
```

y configura las entidades.

Utiliza Fluent API cuando tenga sentido.

Por ejemplo:

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Movie> Movies => Set<Movie>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Genre> Genres => Set<Genre>();

    public DbSet<WatchHistory> WatchHistories => Set<WatchHistory>();

    public DbSet<Favorite> Favorites => Set<Favorite>();
}
```

Explica qué hace DbContext y cómo se relaciona con PostgreSQL.

---

# 8. Migraciones

Configura Entity Framework Core para utilizar migraciones.

Quiero poder hacer conceptualmente:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Explica:

```text
Entidad
 ↓
DbContext
 ↓
Migration
 ↓
PostgreSQL
```

---

# 9. Repositorios

En Application crea interfaces como:

```text
IMovieRepository
IUserRepository
IGenreRepository
IWatchHistoryRepository
IFavoriteRepository
```

Por ejemplo:

```csharp
public interface IMovieRepository
{
    Task<Movie?> GetByIdAsync(Guid id);

    Task<List<Movie>> GetAllAsync();

    Task AddAsync(Movie movie);

    Task DeleteAsync(Guid id);
}
```

Después implementa los repositorios en Infrastructure.

Por ejemplo:

```text
Application
    │
    └── IMovieRepository
              ▲
              │
              │ implementación
              │
Infrastructure
    │
    └── MovieRepository
```

Explica por qué la interfaz no debería depender de PostgreSQL.

---

# 10. Application

Crea servicios de aplicación.

Por ejemplo:

```text
MovieService
UserService
GenreService
FavoriteService
WatchHistoryService
```

Estos servicios deben coordinar los casos de uso.

Ejemplo:

```csharp
public class MovieService
{
    private readonly IMovieRepository _repository;

    public MovieService(IMovieRepository repository)
    {
        _repository = repository;
    }

    public async Task<MovieDto?> GetByIdAsync(Guid id)
    {
        var movie = await _repository.GetByIdAsync(id);

        if (movie == null)
            return null;

        return MovieDto.FromEntity(movie);
    }
}
```

Explica que:

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
Database
```

---

# 11. DTOs

No expongas directamente las entidades del dominio desde la API.

Crea DTOs como:

```text
MovieDto
CreateMovieRequest
UpdateMovieRequest
UserDto
CreateUserRequest
WatchHistoryDto
FavoriteDto
```

Explica la diferencia entre:

```text
Entity
DTO
Request DTO
Response DTO
```

y por qué es conveniente separarlos.

---

# 12. Controllers

Crea controllers:

```text
MoviesController
UsersController
GenresController
FavoritesController
WatchHistoryController
```

Ejemplos de endpoints:

### Movies

```http
GET    /api/movies
GET    /api/movies/{id}
POST   /api/movies
PUT    /api/movies/{id}
DELETE /api/movies/{id}
```

### Genres

```http
GET    /api/genres
GET    /api/genres/{id}
POST   /api/genres
```

### Users

```http
GET    /api/users/{id}
POST   /api/users
```

### Favorites

```http
GET    /api/users/{userId}/favorites
POST   /api/users/{userId}/favorites/{movieId}
DELETE /api/users/{userId}/favorites/{movieId}
```

### Watch History

```http
GET  /api/users/{userId}/history
POST /api/users/{userId}/history
```

No necesitas autenticación JWT inicialmente.

---

# 13. Flujo de una petición

Quiero que implementes y expliques detalladamente este flujo:

```text
GET /api/movies/{id}
        │
        ▼
MoviesController
        │
        ▼
MovieService
        │
        ▼
IMovieRepository
        │
        ▼
MovieRepository
        │
        ▼
AppDbContext
        │
        ▼
PostgreSQL
        │
        ▼
Movie
        │
        ▼
MovieDto
        │
        ▼
HTTP Response
```

Explica qué ocurre en cada paso.

---

# 14. Flujo de creación

También quiero un ejemplo de:

```text
POST /api/movies
```

Flujo:

```text
HTTP Request
    ↓
CreateMovieRequest
    ↓
MoviesController
    ↓
MovieService
    ↓
Validación
    ↓
Movie Entity
    ↓
IMovieRepository
    ↓
MovieRepository
    ↓
PostgreSQL
    ↓
MovieDto
    ↓
HTTP 201
```

---

# 15. Validaciones

Implementa validaciones sencillas.

Ejemplos:

```text
Movie title obligatorio
Movie duration > 0
ReleaseYear válido
Email válido
Genre existente
Movie existente
```

Explica dónde debe realizarse cada tipo de validación.

Distingue entre:

```text
Validación de entrada
Validación de negocio
Validación de persistencia
```

---

# 16. Manejo de errores

Implementa un manejo global de excepciones.

Por ejemplo:

```text
404 → Recurso no encontrado
400 → Datos inválidos
409 → Conflicto
500 → Error inesperado
```

Puedes utilizar middleware.

Quiero entender el flujo:

```text
Controller
 ↓
Exception
 ↓
Global Exception Middleware
 ↓
HTTP Error Response
```

---

# 17. Logging

Agrega logging básico.

Quiero ejemplos de:

```text
Request recibida
Movie consultada
Movie creada
Error ocurrido
```

No necesitas utilizar una plataforma externa de logs.

Utiliza el mecanismo estándar de .NET.

---

# 18. Swagger

Configura Swagger/OpenAPI.

Quiero poder probar los endpoints desde Swagger.

Explica cómo se relaciona:

```text
Controller
 ↓
OpenAPI
 ↓
Swagger UI
```

---

# 19. Tests unitarios

Utiliza xUnit.

Crea pruebas para:

```text
MovieService
UserService
FavoriteService
WatchHistoryService
```

No es necesario probar PostgreSQL en los tests unitarios.

Utiliza mocks/fakes cuando corresponda.

Ejemplo conceptual:

```text
MovieService
      │
      ▼
IMovieRepository
      │
      └── Mock
```

Explica qué se está probando realmente.

---

# 20. Tests de integración

Crea al menos un ejemplo de test de integración que muestre conceptualmente:

```text
HTTP Request
 ↓
Controller
 ↓
Application
 ↓
Infrastructure
 ↓
Database
```

No hace falta crear un sistema complejo de testing.

Si utilizar una base de datos real para integración requiere demasiada configuración, explica qué sería necesario.

---

# 21. Inyección de dependencias

Configura las dependencias en:

```text
Program.cs
```

Ejemplo conceptual:

```csharp
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<MovieService>();
```

Explica:

```text
¿Quién solicita la dependencia?
¿Quién la implementa?
¿Quién la crea?
¿Por qué no hacemos new MovieRepository() dentro del Controller?
```

---

# 22. Configuración

Utiliza:

```text
appsettings.json
appsettings.Development.json
```

para configurar la conexión a PostgreSQL.

No hardcodees credenciales dentro de las clases.

Explica cómo funciona:

```text
Configuration
      ↓
Connection String
      ↓
DbContext
      ↓
PostgreSQL
```

---

# 23. Seguridad

No implementes JWT inicialmente.

Pero crea una sección en el README explicando:

```text
¿Qué añadiríamos posteriormente?
```

Por ejemplo:

```text
JWT
Authentication
Authorization
Password hashing
Refresh tokens
Roles
```

Aclara que esto es una evolución futura.

---

# 24. README

Crea un README.md completo que explique:

## Proyecto

Qué es StreamFlix.

## Tecnologías

Lista de tecnologías utilizadas.

## Arquitectura

Explicación de cada capa.

## Estructura

Árbol de carpetas.

## Requisitos

Qué debe tener instalado el desarrollador.

## Ejecución

Pasos:

```bash
docker compose up -d
dotnet restore
dotnet build
dotnet ef database update
dotnet run
```

Ajusta los comandos si es necesario.

## Swagger

Indica cómo acceder a Swagger.

## Base de datos

Explica cómo conectarse a PostgreSQL.

## Endpoints

Lista los endpoints principales.

## Flujo

Explica el recorrido de una petición.

## Tests

Explica cómo ejecutarlos.

---

# 25. Reglas de arquitectura

Quiero que respetes estas reglas:

### API

La API:

- recibe HTTP;
- valida entrada básica;
- llama a Application;
- devuelve respuestas HTTP.

No debe contener lógica de negocio compleja.

### Application

Application:

- coordina casos de uso;
- utiliza interfaces;
- transforma DTOs;
- coordina dominio e infraestructura mediante abstracciones.

### Domain

Domain:

- contiene entidades;
- reglas de negocio;
- interfaces relacionadas con el dominio cuando sea apropiado.

No debe depender de:

```text
ASP.NET
Entity Framework
PostgreSQL
Docker
HTTP
```

### Infrastructure

Infrastructure:

- implementa repositorios;
- configura Entity Framework;
- conecta con PostgreSQL;
- implementa servicios externos.

### Tests

Los tests deben estar separados del código productivo.

---

# 26. No sobreingenierizar

Aunque quiero una arquitectura profesional, **NO quiero una arquitectura innecesariamente complicada**.

No agregues:

- microservicios;
- CQRS;
- MediatR;
- Event Sourcing;
- Kafka;
- RabbitMQ;
- Kubernetes;
- arquitectura distribuida;

a menos que sea necesario.

El objetivo es aprender correctamente:

```text
API
 ↓
Application
 ↓
Domain
 ↓
Infrastructure
 ↓
Database
```

---

# 27. Orden de generación

No generes todo de golpe sin explicarlo.

Quiero que construyas el proyecto siguiendo este orden:

### Fase 1

Crear solución y proyectos.

### Fase 2

Crear Domain.

### Fase 3

Crear Application.

### Fase 4

Crear Infrastructure.

### Fase 5

Crear API.

### Fase 6

Configurar PostgreSQL + Docker.

### Fase 7

Configurar Entity Framework.

### Fase 8

Crear migraciones.

### Fase 9

Crear endpoints.

### Fase 10

Agregar manejo de errores.

### Fase 11

Agregar Swagger.

### Fase 12

Agregar tests.

### Fase 13

Crear README.

Después de cada fase:

1. Explica qué se creó.
2. Explica por qué se creó.
3. Muestra el código.
4. Explica cómo se conecta con lo anterior.
5. Muestra el flujo.

---

# 28. Lo más importante: quiero aprender

No quiero solamente copiar y pegar código.

Cada vez que crees algo importante, explícame:

```text
¿Qué es?
¿Por qué existe?
¿Quién lo utiliza?
¿De quién depende?
¿Qué ocurriría si no existiera?
```

Por ejemplo, si creas:

```text
IMovieRepository
```

explica:

```text
Controller
    ↓
MovieService
    ↓
IMovieRepository
    ↓
MovieRepository
    ↓
PostgreSQL
```

y por qué `MovieService` utiliza una interfaz en lugar de conocer directamente `MovieRepository`.

---

# 29. Relación con un proyecto empresarial

Al final quiero una sección:

# ¿Qué estoy aprendiendo que podría aplicar a DDL-LIMA?

Relaciona el proyecto StreamFlix con conceptos como:

```text
Movie
      ≈
Entidad de dominio

MovieService
      ≈
Caso de uso / Application Service

ValidationRule
      ≈
Regla de validación DDL

WatchHistory
      ≈
Trazabilidad / histórico

Repository
      ≈
Persistencia

LimsService
      ≈
Integración con sistemas externos

Incident
      ≈
Resultado de una regla que requiere revisión
```

Aclara que estas equivalencias son **conceptuales para aprender arquitectura** y no significan que StreamFlix tenga la misma arquitectura o modelo de datos que DDL-LIMA.

---

# 30. Resultado final

Al finalizar quiero tener:

```text
StreamFlix/
│
├── API
├── Application
├── Domain
├── Infrastructure
├── Unit Tests
├── Integration Tests
├── PostgreSQL
├── Docker
├── Swagger
├── Entity Framework
├── Migrations
└── README
```

Y quiero poder seguir mentalmente este flujo:

```text
CLIENT
   │
   ▼
CONTROLLER
   │
   ▼
APPLICATION SERVICE
   │
   ▼
DOMAIN / BUSINESS RULES
   │
   ▼
REPOSITORY INTERFACE
   │
   ▼
REPOSITORY IMPLEMENTATION
   │
   ▼
ENTITY FRAMEWORK
   │
   ▼
POSTGRESQL
   │
   ▼
RESPONSE DTO
   │
   ▼
CLIENT
```

El proyecto debe ser suficientemente sencillo para que un desarrollador que está aprendiendo arquitectura pueda entenderlo, pero suficientemente profesional para que las decisiones de estructura puedan servir como referencia para proyectos empresariales posteriores.