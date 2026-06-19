using Keycloak.AuthServices.Sdk.Admin;
using Keycloak.AuthServices.Sdk.Admin.Models;
using Keycloak.AuthServices.Sdk.Admin.Requests.Users;
using Microsoft.Extensions.Configuration;
using Passport.Application.Abstractions.Clients;
using Passport.Application.Abstractions.Services;
using Passport.Application.Models;
using Passport.Domain.ValueObjects;

namespace Passport.Application.Services;

public sealed class AuthService(
    IKeycloakUserClient keycloakUserClient,
    IKeycloakTokenClient tokenClient,
    IConfiguration configuration) : IAuthService
{
    private string Realm { get; } = configuration["Keycloak:Realm"]
        ?? throw new InvalidOperationException("Keycloak:Realm is not configured.");

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

    public Task<AuthToken?> LoginAsync(LoginUserCommand command, CancellationToken cancellationToken = default)
    {
        var email = Email.Create(command.Email);
        var password = NormalizeRequired(command.Password, nameof(command.Password));
        return tokenClient.SignInAsync(email.Value, password, cancellationToken);
    }

    public Task<AuthToken?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var normalizedToken = NormalizeRequired(refreshToken, nameof(refreshToken));
        return tokenClient.RefreshTokenAsync(normalizedToken, cancellationToken);
    }

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
