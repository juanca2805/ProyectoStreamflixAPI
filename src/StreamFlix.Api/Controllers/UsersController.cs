using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamFlix.Api.Authorization;
using StreamFlix.Application.Users;
using StreamFlix.Application.Users.Dtos;

namespace StreamFlix.Api.Controllers;

/// <summary>
/// Permisos: crear usuarios es tarea de Admin (no hay registro público, a
/// propósito, por ahora). Consultar un usuario lo puede hacer él mismo o un
/// Admin: el parámetro de ruta se llama {userId} (no {id}) porque es el nombre
/// que OwnerOrAdminHandler busca para comparar contra el token.
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>GET /api/users/{userId} (el propio usuario o Admin)</summary>
    [HttpGet("{userId:guid}")]
    [Authorize(Policy = Policies.OwnerOrAdmin)]
    public async Task<ActionResult<UserDto>> GetById(Guid userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        return Ok(user);
    }

    /// <summary>POST /api/users (solo Admin)</summary>
    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request)
    {
        var user = await _userService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { userId = user.Id }, user);
    }
}
