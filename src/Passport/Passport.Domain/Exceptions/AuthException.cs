namespace Passport.Domain.Exceptions;

/// <summary>Base class for expected authentication domain errors.</summary>
public abstract class AuthException(string message) : Exception(message);
