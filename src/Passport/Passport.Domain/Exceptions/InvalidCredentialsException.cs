namespace Passport.Domain.Exceptions;

/// <summary>Thrown when login credentials are invalid.</summary>
public sealed class InvalidCredentialsException() : AuthException("Неверный email или пароль.");
