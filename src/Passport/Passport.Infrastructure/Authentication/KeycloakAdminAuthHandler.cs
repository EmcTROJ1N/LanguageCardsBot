using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Passport.Infrastructure.Authentication;

public sealed class KeycloakAdminAuthHandler(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<KeycloakAdminAuthHandler> logger) : DelegatingHandler
{
    private string? _cachedToken;
    private DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private string TokenEndpoint =>
        $"{configuration["Keycloak:AuthServerUrl"]}/realms/{configuration["Keycloak:Realm"]}/protocol/openid-connect/token";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await GetTokenAsync(cancellationToken);
        if (token is not null)
            request.Headers.Authorization = new("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string?> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
            return _cachedToken;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
                return _cachedToken;

            var client = httpClientFactory.CreateClient("keycloak-token");
            var response = await client.PostAsync(TokenEndpoint, new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = configuration["Keycloak:Resource"] ?? "",
                    ["client_secret"] = configuration["Keycloak:Credentials:Secret"] ?? ""
                }), cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to obtain Keycloak admin token: {Status}", response.StatusCode);
                return null;
            }

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>(
                cancellationToken: cancellationToken);

            if (token is null) return null;

            _cachedToken = token.AccessToken;
            _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn - 30);
            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
