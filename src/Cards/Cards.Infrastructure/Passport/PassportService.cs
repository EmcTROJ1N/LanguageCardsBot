using Cards.Application.Abstractions.Services;
using Cards.Application.Passport;
using Cards.Infrastructure.Passport.Generated;

namespace Cards.Infrastructure.Passport;

/// <summary>
/// Implements <see cref="IPassportService"/> by delegating to the generated Passport HTTP client.
/// </summary>
internal sealed class PassportService(IPassportClient client) : IPassportService
{
    /// <inheritdoc/>
    public async Task<PassportUserResult?> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.GetUserByIdAsync(userId, cancellationToken);
            return new PassportUserResult(
                response.Id,
                response.Email,
                response.FirstName,
                response.LastName,
                response.Role);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }
}
