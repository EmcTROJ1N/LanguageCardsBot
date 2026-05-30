using Passport.Application.Abstractions.Repositories;
using Passport.Application.Abstractions.Services;
using Passport.Application.Models;
using Passport.Domain.ValueObjects;

namespace Passport.Application.Services;

/// <summary>
/// Implements passport authentication use cases.
/// </summary>
public sealed class AuthService(IKeycloakRepository keycloakRepository) : IAuthService
{
    /// <inheritdoc />
    public async Task<bool> RegisterAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = Email.Create(command.Email);
        var password = NormalizeRequired(command.Password, nameof(command.Password));
        var firstName = NormalizeRequired(command.FirstName, nameof(command.FirstName));
        var lastName = NormalizeRequired(command.LastName, nameof(command.LastName));

        if (await keycloakRepository.UserExistsByEmailAsync(email.Value, cancellationToken))
            return false;

        var user = await keycloakRepository.CreateUserAsync(
            email.Value,
            password,
            firstName,
            lastName,
            cancellationToken);

        return user is not null;
    }

    /// <inheritdoc />
    public Task<AuthToken?> LoginAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = Email.Create(command.Email);
        var password = NormalizeRequired(command.Password, nameof(command.Password));

        return keycloakRepository.SignInAsync(email.Value, password, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AuthToken?> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var normalizedRefreshToken = NormalizeRequired(refreshToken, nameof(refreshToken));
        return keycloakRepository.RefreshTokenAsync(normalizedRefreshToken, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AuthUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return keycloakRepository.GetUserByIdAsync(userId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AuthUser?> GetUserByAccessTokenAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var normalizedAccessToken = NormalizeRequired(accessToken, nameof(accessToken));
        return keycloakRepository.GetUserByAccessTokenAsync(normalizedAccessToken, cancellationToken);
    }

    /// <summary>
    /// Trims and validates a required string value.
    /// </summary>
    /// <param name="value">Raw value.</param>
    /// <param name="parameterName">Parameter name used in validation errors.</param>
    /// <returns>Trimmed value.</returns>
    private static string NormalizeRequired(string? value, string parameterName)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException($"{parameterName} is required.", parameterName);

        return normalized;
    }
}
