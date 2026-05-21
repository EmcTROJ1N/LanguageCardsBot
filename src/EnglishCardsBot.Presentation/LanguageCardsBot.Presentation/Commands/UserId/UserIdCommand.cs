namespace EnglishCardsBot.Presentation.Commands.UserId;

/// <summary>
/// Represents a request to show the current LanguageCardsBot user identifier.
/// </summary>
public sealed record UserIdCommand(long ChatId);
