using Microsoft.AspNetCore.Mvc;
using StreamFlix.Application.Genres;
using StreamFlix.Application.Genres.Dtos;

namespace StreamFlix.Api.Controllers;

[ApiController]
[Route("api/genres")]
public class GenresController : ControllerBase
{
    private readonly GenreService _genreService;

    public GenresController(GenreService genreService)
    {
        _genreService = genreService;
    }

    /// <summary>GET /api/genres</summary>
    [HttpGet]
    public async Task<ActionResult<List<GenreDto>>> GetAll()
    {
        var genres = await _genreService.GetAllAsync();
        return Ok(genres);
    }

    /// <summary>GET /api/genres/{id}</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GenreDto>> GetById(Guid id)
    {
        var genre = await _genreService.GetByIdAsync(id);
        return Ok(genre);
    }

    /// <summary>POST /api/genres</summary>
    [HttpPost]
    public async Task<ActionResult<GenreDto>> Create(CreateGenreRequest request)
    {
        var genre = await _genreService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = genre.Id }, genre);
    }
}
