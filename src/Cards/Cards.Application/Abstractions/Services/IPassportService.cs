using Cards.Application.Passport;

namespace Cards.Application.Abstractions.Services;

/// <summary>
/// Provides user profile data from the Passport identity service.
/// </summary>
public interface IPassportService
{
    /// <summary>
    /// Returns the profile of the user identified by <paramref name="userId"/>,
    /// or <c>null</c> if the user does not exist in Passport.
    /// </summary>
    /// <param name="userId">Keycloak user identifier (JWT <c>sub</c> claim).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PassportUserResult?> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
