
namespace LanguageCardsBot.Contracts.Messaging.Settings;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Shared RabbitMQ connection options. Used by both producer (Cards) and consumer (LanguageCardsBot) services.
/// Bind from configuration section <c>RabbitMq</c>.
/// </summary>
public class RabbitMqOptions
{
    /// <summary>Configuration section key.</summary>
    public const string Section = "RabbitMq";

    /// <summary>RabbitMQ broker host name or IP address.</summary>
    [Required(ErrorMessage = "RabbitMQ HostName обязателен")]
    [MinLength(1)]
    public string HostName { get; set; } = "localhost";

    /// <summary>RabbitMQ user name.</summary>
    [Required(ErrorMessage = "RabbitMQ UserName обязателен")]
    [MinLength(1)]
    public string UserName { get; set; } = "guest";

    /// <summary>RabbitMQ password.</summary>
    [Required(ErrorMessage = "RabbitMQ Password обязателен")]
    public string Password { get; set; } = "guest";

    /// <summary>Exchange name used for all domain events.</summary>
    [Required(ErrorMessage = "ExchangeName обязателен")]
    [RegularExpression(@"^[a-z0-9\-\.]+$",
        ErrorMessage = "ExchangeName может содержать только строчные буквы, цифры, дефис и точку")]
    public string ExchangeName { get; set; } = "domain-events";
}
