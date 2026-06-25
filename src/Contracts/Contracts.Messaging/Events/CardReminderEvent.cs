namespace LanguageCardsBot.Contracts.Messaging.Events;

/// <summary>
/// Published by the Cards microservice when a due card is ready to be shown to the user.
/// The consumer is responsible for formatting and delivering the Telegram message.
/// </summary>
public sealed record CardReminderEvent(
    long ChatId,
    int CardId,
    string Term,
    string Translation,
    bool HideTranslation);
