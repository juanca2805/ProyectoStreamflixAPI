# StreamFlix

Proyecto backend **educativo**, inspirado conceptualmente en una plataforma como Netflix, construido para practicar una **arquitectura profesional en C#/.NET** — el tipo de estructura que después se puede reutilizar en un proyecto empresarial real (ver la sección [¿Qué estoy aprendiendo que podría aplicar a DDL-LIMA?](#qué-estoy-aprendiendo-que-podría-aplicar-a-ddl-lima)).

**No es un clon visual de Netflix.** No tiene frontend. El objetivo es la arquitectura, no la cantidad de funcionalidades.

---

## Tecnologías

| Tecnología | Uso |
|---|---|
| C# / .NET 10 | Lenguaje y runtime |
| ASP.NET Core Web API | API REST |
| PostgreSQL | Base de datos relacional |
| Docker / Docker Compose | Levantar PostgreSQL de forma reproducible |
| Entity Framework Core + Npgsql | ORM y acceso a datos |
| Swagger / Swashbuckle | Documentación interactiva de la API |
| xUnit + Moq | Tests unitarios |
| Microsoft.AspNetCore.Mvc.Testing | Tests de integración |

---

## Arquitectura

Arquitectura por capas inspirada en Clean Architecture. Cada capa es un **proyecto .NET separado**, no solo una carpeta — así las reglas de dependencia las impone el compilador, no la buena voluntad de quien programa.

```
                    ┌──────────────┐
                    │    Client    │
                    └──────┬───────┘
                           ▼
                    ┌──────────────┐
                    │     API      │   Controllers: HTTP ↔ Application
                    └──────┬───────┘
                           ▼
                  ┌──────────────────┐
                  │   Application    │   Casos de uso, DTOs, interfaces de repositorio
                  └────────┬─────────┘
                           ▼
                  ┌──────────────────┐
                  │      Domain      │   Entidades y reglas de negocio puras
                  └────────┬─────────┘
                           ▼
                ┌─────────────────────┐
                │   Infrastructure    │   EF Core, PostgreSQL, repositorios concretos
                └─────────────────────┘
```

### StreamFlix.Domain

- **Responsabilidad:** entidades (`Movie`, `Genre`, `User`, `WatchHistory`, `Favorite`) y sus reglas de negocio (ej. un título no puede estar vacío, la duración debe ser > 0).
- **Qué puede conocer:** solo tipos de .NET (`string`, `Guid`, `List<T>`...).
- **Qué NO debería conocer:** ASP.NET Core, Entity Framework, PostgreSQL, HTTP, Docker.
- **Qué proyectos puede usar:** ninguno del propio repositorio.
- **Por qué:** es el núcleo. Si cambia PostgreSQL por otro motor, o REST por gRPC, las reglas de negocio no deberían tener que tocarse.

### StreamFlix.Application

- **Responsabilidad:** coordinar casos de uso (`MovieService`, `UserService`...), definir DTOs y las interfaces de repositorio (`IMovieRepository`...).
- **Qué puede conocer:** `Domain`.
- **Qué NO debería conocer:** EF Core, PostgreSQL, ASP.NET Core (controllers, HTTP).
- **Qué proyectos puede usar:** `Domain`.
- **Por qué:** permite testear la lógica de aplicación sin base de datos real (con mocks de las interfaces), y cambiar de motor de persistencia sin tocar los casos de uso.

### StreamFlix.Infrastructure

- **Responsabilidad:** implementar las interfaces de `Application` con tecnología concreta: `AppDbContext`, configuraciones Fluent API, repositorios EF Core, hashing de contraseñas.
- **Qué puede conocer:** `Domain`, `Application`, EF Core, Npgsql.
- **Qué NO debería conocer:** ASP.NET Core / HTTP (no sabe qué es un `Controller`).
- **Qué proyectos puede usar:** `Domain`, `Application`.
- **Por qué:** aísla todos los detalles técnicos de persistencia en un solo lugar reemplazable.

### StreamFlix.Api

- **Responsabilidad:** recibir HTTP, delegar a `Application`, devolver respuestas HTTP. Contiene el middleware de manejo de errores y la configuración de Swagger/DI en `Program.cs`.
- **Qué puede conocer:** `Application`, `Infrastructure` (solo para registrar sus servicios en el contenedor de DI).
- **Qué NO debería contener:** lógica de negocio.
- **Por qué:** mantiene el "borde" HTTP separado de las reglas internas — la API es reemplazable por una CLI o un worker sin tocar Domain/Application.

### StreamFlix.UnitTests / StreamFlix.IntegrationTests

- **Responsabilidad:** verificar el comportamiento de `Application` (unitarios, con mocks) y del sistema completo (integración, con `WebApplicationFactory` + PostgreSQL real).
- **Por qué separados del código productivo:** para que nunca se publiquen ni se mezclen con el runtime de producción.

---

## Estructura

```
StreamFlix/
│
├── src/
│   ├── StreamFlix.Api/              Controllers, Program.cs, middleware, appsettings
│   ├── StreamFlix.Application/      Servicios, DTOs, interfaces de repositorio
│   ├── StreamFlix.Domain/           Entidades, reglas de negocio, excepciones de dominio
│   └── StreamFlix.Infrastructure/   AppDbContext, migraciones, repositorios EF Core
│
├── tests/
│   ├── StreamFlix.UnitTests/        Tests de servicios con Moq
│   └── StreamFlix.IntegrationTests/ Tests end-to-end con WebApplicationFactory
│
├── docker-compose.yml               PostgreSQL
├── README.md
└── StreamFlix.sln
```

---

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/) o superior
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para PostgreSQL)
- Herramienta `dotnet-ef`: `dotnet tool install --global dotnet-ef`

---

## Ejecución

```bash
# 1. Levantar PostgreSQL
docker compose up -d

# 2. Restaurar dependencias
dotnet restore

# 3. Compilar
dotnet build

# 4. Aplicar migraciones (crea las tablas en PostgreSQL)
cd src/StreamFlix.Api
dotnet ef database update --project ../StreamFlix.Infrastructure --startup-project .

# 5. Ejecutar la API
dotnet run
```

La API queda escuchando en la URL que indique la consola (ver `Properties/launchSettings.json`, por defecto algo como `http://localhost:5188`).

---

## Swagger

Con la API corriendo en modo Development, abre:

```
http://localhost:<puerto>/swagger
```

Desde ahí puedes probar cada endpoint directamente (Try it out), sin necesidad de Postman ni curl.

---

## Base de datos

PostgreSQL corre en un contenedor Docker (`docker-compose.yml`):

| Variable | Valor |
|---|---|
| Host | `localhost` |
| Puerto | `5432` |
| Base de datos | `streamflix` |
| Usuario | `streamflix_user` |
| Contraseña | `streamflix_password` |

Puedes conectarte con cualquier cliente PostgreSQL (DBeaver, pgAdmin, `psql`) usando esos datos, o inspeccionar directamente:

```bash
docker exec -it streamflix-postgres psql -U streamflix_user -d streamflix
```

---

## Endpoints

```
GET    /api/movies
GET    /api/movies/{id}
POST   /api/movies
PUT    /api/movies/{id}
DELETE /api/movies/{id}

GET    /api/genres
GET    /api/genres/{id}
POST   /api/genres

GET    /api/users/{id}
POST   /api/users

GET    /api/users/{userId}/favorites
POST   /api/users/{userId}/favorites/{movieId}
DELETE /api/users/{userId}/favorites/{movieId}

GET    /api/users/{userId}/history
POST   /api/users/{userId}/history
```

No implementa autenticación JWT (ver [Seguridad](#seguridad)).

---

## Flujo de una petición

### Lectura: `GET /api/movies/{id}`

```
GET /api/movies/{id}
        │
        ▼
MoviesController.GetById(id)         — recibe la petición HTTP
        │
        ▼
MovieService.GetByIdAsync(id)        — caso de uso: "obtener una película"
        │
        ▼
IMovieRepository.GetByIdAsync(id)    — interfaz (Application no sabe cómo se resuelve)
        │
        ▼
MovieRepository.GetByIdAsync(id)     — implementación concreta (Infrastructure)
        │
        ▼
AppDbContext                         — traduce a SQL
        │
        ▼
PostgreSQL                           — ejecuta la consulta
        │
        ▼
Movie (entidad)                      — EF Core reconstruye el objeto
        │
        ▼
MovieDto.FromEntity(movie)           — se convierte a DTO (nunca se expone la entidad)
        │
        ▼
HTTP 200 + JSON
```

### Escritura: `POST /api/movies`

```
HTTP Request (JSON)
    ↓
CreateMovieRequest                — deserializado por ASP.NET Core
    ↓
MoviesController.Create(request)
    ↓
MovieService.CreateAsync(request)
    ↓
Validación de negocio             — géneros existen (Application) +
                                     título/duración/año válidos (Domain, en el constructor de Movie)
    ↓
new Movie(...)                    — entidad construida (o falla con ArgumentException)
    ↓
IMovieRepository.AddAsync(movie)
    ↓
MovieRepository (Infrastructure)
    ↓
PostgreSQL (INSERT vía SaveChangesAsync)
    ↓
MovieDto.FromEntity(movie)
    ↓
HTTP 201 Created
```

Si algo falla en cualquier punto de este flujo, la excepción se propaga sin `try/catch` locales hasta el `ExceptionHandlingMiddleware`, que la traduce al código HTTP correspondiente (`400`, `404`, `409` o `500`).

---

## Tests

```bash
# Todos los tests (unitarios + integración)
dotnet test

# Solo unitarios
dotnet test tests/StreamFlix.UnitTests

# Solo integración (requiere PostgreSQL corriendo: docker compose up -d)
dotnet test tests/StreamFlix.IntegrationTests
```

- **Unitarios:** prueban los `Service` de `Application` con repositorios simulados (Moq). No requieren PostgreSQL.
- **Integración:** arrancan la API completa en memoria (`WebApplicationFactory<Program>`) y hacen peticiones HTTP reales contra el PostgreSQL de `docker-compose`.

---

## Seguridad

Este proyecto **no implementa autenticación ni autorización** — está fuera de alcance a propósito, para mantener el foco en la arquitectura de capas.

Una evolución futura y realista añadiría:

```
JWT (JSON Web Tokens)     — para autenticar al cliente en cada petición
Authentication            — verificar "quién eres"
Authorization             — verificar "qué puedes hacer" (roles, políticas)
Password hashing           — ya implementado (PBKDF2), pero sin login que lo use aún
Refresh tokens             — renovar la sesión sin pedir credenciales de nuevo
Roles                      — ej. distinguir un usuario normal de un administrador
```

---

## ¿Qué estoy aprendiendo que podría aplicar a DDL-LIMA?

Estas equivalencias son **conceptuales**, para practicar arquitectura — no implican que StreamFlix comparta modelo de datos ni reglas con DDL-LIMA.

| StreamFlix | Concepto general | Posible equivalente en DDL-LIMA |
|---|---|---|
| `Movie` | Entidad de dominio con reglas propias | Cualquier entidad central del dominio (ej. una muestra, un lote) |
| `MovieService` | Caso de uso / Application Service | Un servicio que orquesta una operación de negocio |
| Validaciones en el constructor de `Movie` | Regla de negocio que no depende de infraestructura | Una regla de validación de negocio en DDL |
| `WatchHistory` | Registro de trazabilidad (qué pasó, cuándo) | Histórico / auditoría de resultados |
| `IMovieRepository` / `MovieRepository` | Abstracción de persistencia + su implementación | Capa de acceso a datos desacoplada del dominio |
| (no implementado aquí) | Integración con sistemas externos | Un servicio tipo `LimsService` que consulta otro sistema |
| `NotFoundException` / `ConflictException` | Resultado de una regla que requiere manejo explícito | Un incidente o excepción que requiere revisión |

Lo que realmente se transfiere de este proyecto a uno empresarial no es el modelo de datos, sino la **forma de separar responsabilidades**: reglas de negocio aisladas de HTTP y de la base de datos, dependencias apuntando siempre "hacia adentro" (Api → Application → Domain), e infraestructura reemplazable sin tocar el núcleo.
