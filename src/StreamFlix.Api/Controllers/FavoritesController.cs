using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamFlix.Api.Authorization;
using StreamFlix.Application.Favorites;
using StreamFlix.Application.Favorites.Dtos;

namespace StreamFlix.Api.Controllers;

/// <summary>
/// Endpoints anidados bajo /api/users/{userId}/favorites: "favoritos" no tiene
/// sentido sin un usuario dueño, así que la ruta lo refleja explícitamente.
///
/// Permisos: OwnerOrAdmin en todo el controller — un usuario solo gestiona SUS
/// favoritos (el {userId} de la ruta debe coincidir con su token); Admin, los de cualquiera.
/// </summary>
[ApiController]
[Route("api/users/{userId:guid}/favorites")]
[Authorize(Policy = Policies.OwnerOrAdmin)]
public class FavoritesController : ControllerBase
{
    private readonly FavoriteService _favoriteService;

    public FavoritesController(FavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    /// <summary>GET /api/users/{userId}/favorites</summary>
    [HttpGet]
    public async Task<ActionResult<List<FavoriteDto>>> GetAll(Guid userId)
    {
        var favorites = await _favoriteService.GetByUserIdAsync(userId);
        return Ok(favorites);
    }

    /// <summary>POST /api/users/{userId}/favorites/{movieId}</summary>
    [HttpPost("{movieId:guid}")]
    public async Task<ActionResult<FavoriteDto>> Add(Guid userId, Guid movieId)
    {
        var favorite = await _favoriteService.AddAsync(userId, movieId);
        return CreatedAtAction(nameof(GetAll), new { userId }, favorite);
    }

    /// <summary>DELETE /api/users/{userId}/favorites/{movieId}</summary>
    [HttpDelete("{movieId:guid}")]
    public async Task<IActionResult> Remove(Guid userId, Guid movieId)
    {
        await _favoriteService.RemoveAsync(userId, movieId);
        return NoContent();
    }
}
