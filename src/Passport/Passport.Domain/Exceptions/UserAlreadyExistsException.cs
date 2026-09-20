namespace Passport.Domain.Exceptions;

/// <summary>Thrown when a user with the same email is already registered.</summary>
public sealed class UserAlreadyExistsException() : AuthException("Пользователь с таким email уже зарегистрирован.");
