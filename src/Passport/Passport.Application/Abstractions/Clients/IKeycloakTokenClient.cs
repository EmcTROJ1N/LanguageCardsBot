using Passport.Application.Models;

namespace Passport.Application.Abstractions.Clients;

public interface IKeycloakTokenClient
{
    Task<AuthToken?> SignInAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AuthToken?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
