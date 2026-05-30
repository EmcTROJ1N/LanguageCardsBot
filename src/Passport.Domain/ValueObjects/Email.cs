using System.Net.Mail;

namespace Passport.Domain.ValueObjects;

/// <summary>
/// Represents a normalized email address used by the passport service.
/// </summary>
public sealed record Email
{
    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the normalized email value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a normalized email value object.
    /// </summary>
    /// <param name="email">Raw email address.</param>
    /// <returns>Normalized email value object.</returns>
    /// <exception cref="ArgumentException">Thrown when the email is empty or invalid.</exception>
    public static Email Create(string? email)
    {
        var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Email is required.", nameof(email));

        try
        {
            var address = new MailAddress(normalized);
            if (!address.Address.Equals(normalized, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Email has invalid format.", nameof(email));
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Email has invalid format.", nameof(email), ex);
        }

        return new Email(normalized);
    }
}
