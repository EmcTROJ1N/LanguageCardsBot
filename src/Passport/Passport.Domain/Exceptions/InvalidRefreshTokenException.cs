namespace Passport.Domain.Exceptions;

/// <summary>Thrown when a refresh token is expired or invalid.</summary>
public sealed class InvalidRefreshTokenException() : AuthException("Сессия истекла, войдите снова.");
