using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using StreamFlix.Domain.Entities;

namespace StreamFlix.Api.Authorization;

/// <summary>
/// Requisito vacío: solo sirve para "nombrar" la regla. La lógica vive en el handler.
/// </summary>
public class OwnerOrAdminRequirement : IAuthorizationRequirement { }

/// <summary>
/// Autoriza si el usuario del token es Admin, o si el valor de ruta {userId}
/// coincide con el Id del usuario autenticado (claim "sub" del JWT).
///
/// Así, en rutas como /api/users/{userId}/favorites, un usuario normal solo
/// puede tocar SUS favoritos: pedir los de otro userId devuelve 403 aunque el
/// token sea válido. Un Admin puede ver/gestionar los de cualquiera.
///
/// Vive en Api (no en Application) porque depende de HttpContext y de cómo
/// están armadas las rutas: es un detalle del borde HTTP, no una regla de negocio.
/// </summary>
public class OwnerOrAdminHandler : AuthorizationHandler<OwnerOrAdminRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OwnerOrAdminHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnerOrAdminRequirement requirement)
    {
        if (context.User.IsInRole(UserRole.Admin.ToString()))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var routeUserId = _httpContextAccessor.HttpContext?.GetRouteValue("userId")?.ToString();
        if (!Guid.TryParse(routeUserId, out var requestedUserId))
            return Task.CompletedTask; // sin {userId} en la ruta no hay "dueño" que comparar: se deniega.

        // JwtTokenGenerator emite el Id como claim "sub". El middleware de JwtBearer,
        // por defecto, lo re-mapea a ClaimTypes.NameIdentifier al construir el
        // ClaimsPrincipal; se aceptan ambos nombres para no depender de ese detalle.
        var tokenUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (Guid.TryParse(tokenUserId, out var authenticatedUserId) && authenticatedUserId == requestedUserId)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
