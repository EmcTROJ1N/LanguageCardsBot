using LanguageCardsBot.Contracts.Cards.V3;
using LanguageCardsBot.Presentation.Abstractions;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using HandleErrorSource = Telegram.Bot.Polling.HandleErrorSource;
using User = LanguageCardsBot.Contracts.Cards.V3.User;
using UserService = LanguageCardsBot.Contracts.Cards.V3.UserService;

namespace LanguageCardsBot.Presentation.Services;

/// <summary>
/// Thin router: receives Telegram updates and delegates to the appropriate dispatcher or handler.
/// Contains no command or callback logic.
/// </summary>
public class TelegramBotService(
    ITelegramBotClient botClient,
    UserService.UserServiceClient userService,
    ICommandDispatcher commandDispatcher,
    ICallbackDispatcher callbackDispatcher,
    IDocumentHandler documentHandler,
    ICardInputHandler cardInputHandler,
    IConfiguration configuration)
{
    /// <summary>Starts the Telegram long-polling loop.</summary>
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        botClient.StartReceiving(
            updateHandler: new BotUpdateHandler(this),
            cancellationToken: cancellationToken);
        return Task.CompletedTask;
    }

    /// <summary>Entry point for all incoming Telegram updates.</summary>
    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken ct)
    {
        if (update.Message is { } message)
            await HandleMessageAsync(message, ct);
        else if (update.CallbackQuery is { } callbackQuery)
            await HandleCallbackQueryAsync(callbackQuery, ct);
    }

    private async Task HandleMessageAsync(Message message, CancellationToken ct)
    {
        var user = await ResolveUserAsync(message.Chat.Id, ct);
        if (user is null)
        {
            await SendRegistrationPromptAsync(message.Chat.Id, ct);
            return;
        }

        if (message.Document is { } document)
        {
            await documentHandler.HandleAsync(message, document, user, ct);
            return;
        }

        if (message.Text is not { } text) return;

        if (text.StartsWith('/'))
        {
            var parts = text.Split(' ');
            await commandDispatcher.TryDispatchAsync(parts[0].ToLower(), message.Chat.Id, parts[1..], user, ct);
        }
        else if (!await commandDispatcher.TryDispatchAsync(text, message.Chat.Id, [], user, ct))
        {
            await cardInputHandler.HandleAsync(message, text, user, ct);
        }
    }

    private async Task HandleCallbackQueryAsync(CallbackQuery cb, CancellationToken ct)
    {
        await botClient.AnswerCallbackQuery(cb.Id, cancellationToken: ct);
        var user = await ResolveUserAsync(cb.Message!.Chat.Id, ct);
        if (user is null)
        {
            await SendRegistrationPromptAsync(cb.Message!.Chat.Id, ct);
            return;
        }
        await callbackDispatcher.TryDispatchAsync(cb, user, ct);
    }

    /// <summary>Looks up the Cards-service user by Telegram chat identifier. Returns null if no account is linked.</summary>
    private async Task<User?> ResolveUserAsync(long chatId, CancellationToken ct)
    {
        var response = await userService.GetByChatIdAsync(
            new GetUserByChatIdRequest { ChatId = chatId },
            cancellationToken: ct);
        return response.User;
    }

    private async Task SendRegistrationPromptAsync(long chatId, CancellationToken ct)
    {
        var siteUrl = configuration["Bot:SiteUrl"] ?? "https://languagecards.app";
        await botClient.SendMessage(
            chatId: chatId,
            text: $"Ваш Telegram ID: {chatId}\n\n" +
                  $"Чтобы пользоваться ботом, зарегистрируйтесь на сайте:\n{siteUrl}\n\n" +
                  "После регистрации введите ваш Telegram ID в настройках профиля.",
            cancellationToken: ct);
    }

    internal Task HandlePollingErrorAsync(ITelegramBotClient _, Exception exception, CancellationToken __)
    {
        Console.WriteLine($"Polling error: {exception.Message}");
        return Task.CompletedTask;
    }

    private class BotUpdateHandler(TelegramBotService botService) : IUpdateHandler
    {
        public Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken ct)
            => botService.HandleUpdateAsync(client, update, ct);

        public Task HandleErrorAsync(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken ct)
            => botService.HandlePollingErrorAsync(client, exception, ct);
    }
}
