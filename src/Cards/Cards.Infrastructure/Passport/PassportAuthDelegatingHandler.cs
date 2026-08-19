using Microsoft.AspNetCore.Http;

namespace Cards.Infrastructure.Passport;

/// <summary>
/// Forwards the current request's Bearer token to outbound Passport API calls.
/// </summary>
internal sealed class PassportAuthDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    : DelegatingHandler
{
    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var authHeader = httpContextAccessor.HttpContext?
            .Request.Headers.Authorization
            .ToString();

        if (!string.IsNullOrEmpty(authHeader))
            request.Headers.TryAddWithoutValidation("Authorization", authHeader);

        return base.SendAsync(request, cancellationToken);
    }
}
