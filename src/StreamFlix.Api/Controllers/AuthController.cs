using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamFlix.Application.Auth;
using StreamFlix.Application.Auth.Dtos;

namespace StreamFlix.Api.Controllers;

/// <summary>
/// Único controller accesible sin token (ver el filtro global de autorización
/// en Program.cs): sin este endpoint nadie podría conseguir un JWT para
/// autenticarse contra el resto de la API.
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>POST /api/auth/login</summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }
}
