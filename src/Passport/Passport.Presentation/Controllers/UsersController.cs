using Microsoft.AspNetCore.Authorization;
using Passport.Application.Abstractions.Services;
using Passport.Presentation.Controllers.Generated;

namespace Passport.Presentation.Controllers;

/// <summary>
/// Exposes service-to-service user profile lookup through the internal REST API.
/// </summary>
public sealed class UsersController(IAuthService authService) : UsersControllerBase
{
    /// <inheritdoc/>
    [Authorize]
    public override async Task<UserResponse> GetUserById(
        Guid id, CancellationToken cancellationToken = default)
    {
        var user = await authService.GetUserByIdAsync(id, cancellationToken);
        if (user is null)
        {
            Response.StatusCode = 404;
            return null!;
        }

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role
        };
    }
}
