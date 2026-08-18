using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamFlix.Api.Authorization;
using StreamFlix.Application.WatchHistories;
using StreamFlix.Application.WatchHistories.Dtos;

namespace StreamFlix.Api.Controllers;

/// <summary>
/// Permisos: OwnerOrAdmin en todo el controller — un usuario solo ve/registra SU
/// historial; Admin, el de cualquiera. Ver FavoritesController para el mismo patrón.
/// </summary>
[ApiController]
[Route("api/users/{userId:guid}/history")]
[Authorize(Policy = Policies.OwnerOrAdmin)]
public class WatchHistoryController : ControllerBase
{
    private readonly WatchHistoryService _watchHistoryService;

    public WatchHistoryController(WatchHistoryService watchHistoryService)
    {
        _watchHistoryService = watchHistoryService;
    }

    /// <summary>GET /api/users/{userId}/history</summary>
    [HttpGet]
    public async Task<ActionResult<List<WatchHistoryDto>>> GetAll(Guid userId)
    {
        var history = await _watchHistoryService.GetByUserIdAsync(userId);
        return Ok(history);
    }

    /// <summary>POST /api/users/{userId}/history</summary>
    [HttpPost]
    public async Task<ActionResult<WatchHistoryDto>> Add(Guid userId, CreateWatchHistoryRequest request)
    {
        var entry = await _watchHistoryService.AddAsync(userId, request);
        return CreatedAtAction(nameof(GetAll), new { userId }, entry);
    }
}
