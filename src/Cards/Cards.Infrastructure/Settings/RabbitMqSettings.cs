namespace Cards.Infrastructure.Settings;

// Infrastructure/Messaging/RabbitMqOptions.cs
using System.ComponentModel.DataAnnotations;

public class RabbitMqOptions
{
    public const string Section = "RabbitMq";

    [Required(ErrorMessage = "RabbitMQ HostName обязателен")]
    [MinLength(1)]
    public string HostName { get; set; } = "localhost";

    [Required(ErrorMessage = "RabbitMQ UserName обязателен")]
    [MinLength(1)]
    public string UserName { get; set; } = "guest";

    [Required(ErrorMessage = "RabbitMQ Password обязателен")]
    public string Password { get; set; } = "guest";

    [Required(ErrorMessage = "ExchangeName обязателен")]
    [RegularExpression(@"^[a-z0-9\-\.]+$",
        ErrorMessage = "ExchangeName может содержать только строчные буквы, цифры, дефис и точку")]
    public string ExchangeName { get; set; } = "domain-events";
}