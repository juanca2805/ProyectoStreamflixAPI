using Microsoft.AspNetCore.Mvc;
using StreamFlix.Application.WatchHistories;
using StreamFlix.Application.WatchHistories.Dtos;

namespace StreamFlix.Api.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/history")]
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
