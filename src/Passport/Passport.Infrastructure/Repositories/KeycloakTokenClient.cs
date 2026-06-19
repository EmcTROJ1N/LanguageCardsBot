using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Passport.Application.Abstractions.Clients;
using Passport.Application.Models;

namespace Passport.Infrastructure.Repositories;

public sealed class KeycloakTokenClient(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : IKeycloakTokenClient
{
    private string TokenEndpoint => configuration["KeycloakPublicClient:TokenEndpoint"]
        ?? throw new InvalidOperationException("KeycloakPublicClient:TokenEndpoint is not configured.");

    private string ClientId => configuration["KeycloakPublicClient:ClientId"]
        ?? throw new InvalidOperationException("KeycloakPublicClient:ClientId is not configured.");

    public async Task<AuthToken?> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("keycloak-token");
        var response = await client.PostAsync(TokenEndpoint, new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = ClientId,
                ["username"] = email,
                ["password"] = password
            }), cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var token = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken: cancellationToken);
        return token is null ? null : MapToken(token);
    }

    public async Task<AuthToken?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("keycloak-token");
        var response = await client.PostAsync(TokenEndpoint, new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = ClientId,
                ["refresh_token"] = refreshToken
            }), cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var token = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken: cancellationToken);
        return token is null ? null : MapToken(token);
    }

    private static AuthToken MapToken(KeycloakTokenResponse r) =>
        new(r.AccessToken, r.RefreshToken, r.ExpiresIn, r.TokenType);

    private sealed record KeycloakTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn,
        [property: JsonPropertyName("token_type")] string TokenType);
}
