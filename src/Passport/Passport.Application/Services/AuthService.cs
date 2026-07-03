using Keycloak.AuthServices.Sdk.Admin;
using Keycloak.AuthServices.Sdk.Admin.Models;
using Keycloak.AuthServices.Sdk.Admin.Requests.Users;
using Microsoft.Extensions.Options;
using Passport.Application.Abstractions.Clients;
using Passport.Application.Abstractions.Services;
using Passport.Application.Models;
using Passport.Application.Options;
using Passport.Domain.ValueObjects;

namespace Passport.Application.Services;

/// <summary>
/// Implements authentication use cases by coordinating Keycloak admin and token clients.
/// </summary>
public sealed class AuthService(
    IKeycloakUserClient keycloakUserClient,
    IKeycloakTokenClient tokenClient,
    IOptions<KeycloakOptions> options) : IAuthService
{
    private string Realm { get; } = options.Value.Realm;

    /// <inheritdoc />
    public async Task<bool> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        var email = Email.Create(command.Email);
        var password = NormalizeRequired(command.Password, nameof(command.Password));
        var firstName = NormalizeRequired(command.FirstName, nameof(command.FirstName));
        var lastName = NormalizeRequired(command.LastName, nameof(command.LastName));

        var existing = await keycloakUserClient.GetUsersAsync(Realm,
            new GetUsersRequestParameters { Email = email.Value, Exact = true },
            cancellationToken);

        if (existing.Any())
            return false;

        await keycloakUserClient.CreateUserAsync(Realm, new UserRepresentation
        {
            Username = email.Value,
            Email = email.Value,
            FirstName = firstName,
            LastName = lastName,
            Enabled = true,
            Credentials =
            [
                new CredentialRepresentation
                {
                    Type = "password",
                    Value = password,
                    Temporary = false
                }
            ]
        }, cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public Task<AuthToken?> LoginAsync(LoginUserCommand command, CancellationToken cancellationToken = default)
    {
        var email = Email.Create(command.Email);
        var password = NormalizeRequired(command.Password, nameof(command.Password));
        return tokenClient.SignInAsync(email.Value, password, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AuthToken?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var normalizedToken = NormalizeRequired(refreshToken, nameof(refreshToken));
        return tokenClient.RefreshTokenAsync(normalizedToken, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AuthUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await keycloakUserClient.GetUserAsync(Realm, userId.ToString(),
                cancellationToken: cancellationToken);
            return MapUser(user);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private static AuthUser MapUser(UserRepresentation user) => new(
        Guid.Parse(user.Id!),
        user.Email!,
        user.FirstName ?? string.Empty,
        user.LastName ?? string.Empty,
        "User",
        DateTimeOffset.FromUnixTimeMilliseconds(user.CreatedTimestamp ?? 0).UtcDateTime);

    private static string NormalizeRequired(string? value, string parameterName)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        return normalized;
    }
}
