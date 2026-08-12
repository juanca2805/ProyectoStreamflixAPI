using Microsoft.AspNetCore.Mvc;
using StreamFlix.Application.Movies;
using StreamFlix.Application.Movies.Dtos;

namespace StreamFlix.Api.Controllers;

/// <summary>
/// Responsabilidad de un Controller: traducir HTTP ↔ llamadas a Application.
/// No contiene reglas de negocio ni sabe nada de EF Core/PostgreSQL.
/// Si algo sale mal (NotFoundException, etc.), NO se atrapa aquí:
/// el middleware global de excepciones (Fase 10) se encarga de convertirlo en HTTP.
/// </summary>
[ApiController]
[Route("api/movies")]
public class MoviesController : ControllerBase
{
    private readonly MovieService _movieService;

    public MoviesController(MovieService movieService)
    {
        _movieService = movieService;
    }

    /// <summary>GET /api/movies</summary>
    [HttpGet]
    public async Task<ActionResult<List<MovieDto>>> GetAll()
    {
        var movies = await _movieService.GetAllAsync();
        return Ok(movies);
    }

    /// <summary>GET /api/movies/{id}</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MovieDto>> GetById(Guid id)
    {
        var movie = await _movieService.GetByIdAsync(id);
        return Ok(movie);
    }

    /// <summary>POST /api/movies</summary>
    [HttpPost]
    public async Task<ActionResult<MovieDto>> Create(CreateMovieRequest request)
    {
        var movie = await _movieService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
    }

    /// <summary>PUT /api/movies/{id}</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MovieDto>> Update(Guid id, UpdateMovieRequest request)
    {
        var movie = await _movieService.UpdateAsync(id, request);
        return Ok(movie);
    }

    /// <summary>DELETE /api/movies/{id}</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _movieService.DeleteAsync(id);
        return NoContent();
    }
}
