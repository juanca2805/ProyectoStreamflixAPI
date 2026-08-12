using System.Net;
using System.Text.Json;
using StreamFlix.Domain.Exceptions;

namespace StreamFlix.Api.Middleware;

/// <summary>
/// Middleware global de excepciones. Envuelve TODA la ejecución de cada petición:
/// si un Controller, Service o Repository lanza una excepción sin atraparla,
/// termina aquí, y aquí decidimos qué código HTTP y qué cuerpo de respuesta enviar.
///
/// Flujo:
///   Controller → (lanza excepción) → ... → ExceptionHandlingMiddleware → HTTP Error Response
///
/// Ventaja principal: ningún Controller necesita try/catch repetido para
/// "traducir" NotFoundException a 404 — se hace una sola vez, en un solo lugar.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Recurso no encontrado"),
            ConflictException => (HttpStatusCode.Conflict, "Conflicto"),
            ArgumentException => (HttpStatusCode.BadRequest, "Datos inválidos"),
            _ => (HttpStatusCode.InternalServerError, "Error inesperado")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            // Solo los errores inesperados (500) se registran como Error:
            // los 400/404/409 son parte del flujo normal de negocio, no fallos del sistema.
            _logger.LogError(exception, "Error no controlado procesando {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning("{ExceptionType}: {Message}", exception.GetType().Name, exception.Message);
        }

        var problemDetails = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            detail = exception.Message,
            traceId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
