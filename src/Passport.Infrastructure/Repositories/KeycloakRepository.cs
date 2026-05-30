using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Passport.Application.Abstractions.Repositories;
using Passport.Application.Models;

namespace Passport.Infrastructure.Repositories;

/// <summary>
/// Temporary in-memory Keycloak repository stub used until real Keycloak integration is added.
/// </summary>
public class KeycloakRepository : IKeycloakRepository
{
    private const int AccessTokenLifetimeSeconds = 3600;
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);
    private static readonly object SyncRoot = new();
    private static readonly ConcurrentDictionary<Guid, StoredUser> UsersById = new();
    private static readonly ConcurrentDictionary<string, Guid> UserIdsByEmail = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, TokenSession> SessionsByAccessToken = new();
    private static readonly ConcurrentDictionary<string, TokenSession> SessionsByRefreshToken = new();

    /// <inheritdoc />
    public Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(UserIdsByEmail.ContainsKey(email));
    }

    /// <inheritdoc />
    public Task<AuthUser?> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (SyncRoot)
        {
            if (UserIdsByEmail.ContainsKey(email))
                return Task.FromResult<AuthUser?>(null);

            var salt = CreateToken();
            var user = new StoredUser(
                Guid.NewGuid(),
                email,
                firstName,
                lastName,
                "User",
                DateTime.UtcNow,
                salt,
                HashPassword(password, salt));

            UsersById[user.Id] = user;
            UserIdsByEmail[email] = user.Id;

            return Task.FromResult<AuthUser?>(MapUser(user));
        }
    }

    /// <inheritdoc />
    public Task<AuthToken?> SignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!UserIdsByEmail.TryGetValue(email, out var userId) || !UsersById.TryGetValue(userId, out var user))
            return Task.FromResult<AuthToken?>(null);

        if (!PasswordMatches(password, user.PasswordSalt, user.PasswordHash))
            return Task.FromResult<AuthToken?>(null);

        return Task.FromResult<AuthToken?>(IssueTokens(user.Id));
    }

    /// <inheritdoc />
    public Task<AuthToken?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!SessionsByRefreshToken.TryRemove(refreshToken, out var session))
            return Task.FromResult<AuthToken?>(null);

        if (session.RefreshTokenExpiresAt <= DateTime.UtcNow)
            return Task.FromResult<AuthToken?>(null);

        if (!UsersById.ContainsKey(session.UserId))
            return Task.FromResult<AuthToken?>(null);

        return Task.FromResult<AuthToken?>(IssueTokens(session.UserId));
    }

    /// <inheritdoc />
    public Task<AuthUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(UsersById.TryGetValue(userId, out var user)
            ? MapUser(user)
            : null);
    }

    /// <inheritdoc />
    public Task<AuthUser?> GetUserByAccessTokenAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!SessionsByAccessToken.TryGetValue(accessToken, out var session))
            return Task.FromResult<AuthUser?>(null);

        if (session.AccessTokenExpiresAt <= DateTime.UtcNow)
        {
            SessionsByAccessToken.TryRemove(accessToken, out _);
            return Task.FromResult<AuthUser?>(null);
        }

        return Task.FromResult(UsersById.TryGetValue(session.UserId, out var user)
            ? MapUser(user)
            : null);
    }

    /// <summary>
    /// Issues a stub access token and refresh token for a user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <returns>Issued token response.</returns>
    private static AuthToken IssueTokens(Guid userId)
    {
        var now = DateTime.UtcNow;
        var accessToken = $"stub_access_{CreateToken()}";
        var refreshToken = $"stub_refresh_{CreateToken()}";
        var session = new TokenSession(
            userId,
            accessToken,
            refreshToken,
            now.AddSeconds(AccessTokenLifetimeSeconds),
            now.Add(RefreshTokenLifetime));

        SessionsByAccessToken[accessToken] = session;
        SessionsByRefreshToken[refreshToken] = session;

        return new AuthToken(accessToken, refreshToken, AccessTokenLifetimeSeconds);
    }

    /// <summary>
    /// Maps a stored user to the application user DTO.
    /// </summary>
    /// <param name="user">Stored user.</param>
    /// <returns>Application user DTO.</returns>
    private static AuthUser MapUser(StoredUser user)
    {
        return new AuthUser(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.CreatedAt);
    }

    /// <summary>
    /// Creates a cryptographically random token fragment.
    /// </summary>
    /// <returns>Lowercase hexadecimal token value.</returns>
    private static string CreateToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
    }

    /// <summary>
    /// Hashes a password with a per-user salt for the local stub store.
    /// </summary>
    /// <param name="password">Raw password.</param>
    /// <param name="salt">Per-user salt.</param>
    /// <returns>Lowercase hexadecimal password hash.</returns>
    private static string HashPassword(string password, string salt)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{salt}:{password}"));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Compares a raw password against a stored salted hash.
    /// </summary>
    /// <param name="password">Raw password.</param>
    /// <param name="salt">Per-user salt.</param>
    /// <param name="passwordHash">Stored password hash.</param>
    /// <returns><c>true</c> when the password matches; otherwise <c>false</c>.</returns>
    private static bool PasswordMatches(string password, string salt, string passwordHash)
    {
        var actual = Convert.FromHexString(HashPassword(password, salt));
        var expected = Convert.FromHexString(passwordHash);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    /// <summary>
    /// Represents a user stored by the in-memory Keycloak stub.
    /// </summary>
    private sealed record StoredUser(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        string Role,
        DateTime CreatedAt,
        string PasswordSalt,
        string PasswordHash);

    /// <summary>
    /// Represents an issued token session in the in-memory stub store.
    /// </summary>
    private sealed record TokenSession(
        Guid UserId,
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiresAt,
        DateTime RefreshTokenExpiresAt);
}
