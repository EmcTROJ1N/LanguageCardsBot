using Microsoft.AspNetCore.Http.Extensions;

namespace Passport.Presentation.Middleware;

/// <summary>
/// Converts unhandled exceptions to JSON error responses.
/// Expected infrastructure errors (e.g. Keycloak unreachable) return 503;
/// all other exceptions return 500.
/// </summary>
internal sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    /// <inheritdoc cref="ExceptionHandlingMiddleware"/>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            HttpRequestException => (503, "Сервис авторизации временно недоступен. Попробуйте позже."),
            _ => (500, "Произошла непредвиденная ошибка. Попробуйте позже.")
        };

        if (statusCode == 500)
            logger.LogError(exception, "Unhandled exception for {Method} {Url}", context.Request.Method, context.Request.GetDisplayUrl());
        else
            logger.LogWarning(exception, "Infrastructure error for {Method} {Url}", context.Request.Method, context.Request.GetDisplayUrl());

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { error = message });
    }
}
