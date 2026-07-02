using LanguageCardsBot.Contracts.Cards.V3;
using LanguageCardsBot.Presentation.Abstractions;
using Telegram.Bot;

namespace LanguageCardsBot.Presentation.Commands.UserId;

/// <summary>
/// Handles the command that shows the current LanguageCardsBot user identifier.
/// </summary>
public sealed class UserIdCommandHandler(ITelegramBotClient botClient) : ICommandHandler<UserIdCommand>
{
    /// <summary>
    /// Sends the current user identifier that can be copied into the Chrome extension settings.
    /// </summary>
    public async Task HandleAsync(
        UserIdCommand command,
        User user,
        CancellationToken cancellationToken = default)
    {
        var message = $"Ваш User ID для расширения:\n{user.Id}\n\n" +
                      "Скопируйте это число в поле User ID в настройках Chrome-расширения.";

        await botClient.SendMessage(
            chatId: command.ChatId,
            text: message,
            cancellationToken: cancellationToken);
    }
}
