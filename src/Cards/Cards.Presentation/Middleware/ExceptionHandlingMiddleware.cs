using Microsoft.AspNetCore.Http.Extensions;

namespace Cards.Presentation.Middleware;

/// <summary>
/// Converts unhandled exceptions to JSON error responses.
/// Infrastructure errors (e.g. DB or upstream unavailable) return 503.
/// Unexpected exceptions return 500 and are logged as errors.
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
            ArgumentException argEx => (400, FirstLine(argEx.Message)),
            HttpRequestException => (503, "Сервис временно недоступен. Попробуйте позже."),
            _ => (500, "Произошла непредвиденная ошибка. Попробуйте позже.")
        };

        if (statusCode == 500)
            logger.LogError(exception, "Unhandled exception for {Method} {Url}", context.Request.Method, context.Request.GetDisplayUrl());
        else if (statusCode == 503)
            logger.LogWarning(exception, "Infrastructure error for {Method} {Url}", context.Request.Method, context.Request.GetDisplayUrl());

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { error = message });
    }

    private static string FirstLine(string message) =>
        message.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)[0];
}
