using System.Net.Http.Headers;
using System.Text.Json;
using EnglishCardsBot.Application.Interfaces;
using EnglishCardsBot.Domain.Entities;
using EnglishCardsBot.Presentation.Abstractions;
using Telegram.Bot;

namespace EnglishCardsBot.Presentation.Commands.Export;

public class ExportCommandHandler(ITelegramBotClient botClient,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ICardRepository cardRepository): ICommandHandler<ExportCommand>
{
    public async Task HandleAsync(ExportCommand command, User user, CancellationToken cancellationToken = default)
    {
        var cards = (await cardRepository.GetAllByUserIdAsync(user.Id, cancellationToken)).ToList();

        if (!cards.Any())
        {
            await botClient.SendMessage(
                chatId: command.ChatId,
                text: "У вас нет карточек для экспорта.",
                cancellationToken: cancellationToken);
            return;
        }

        try
        {
            var exportData = new
            {
                version = "1.0",
                exportedAt = DateTime.UtcNow.ToString("O"),
                totalCards = cards.Count,
                cards = cards.Select(c => new
                {
                    term = c.Term,
                    translation = c.Translation,
                    transcription = c.Transcription,
                    example = c.Example,
                    level = c.Level,
                    learned = c.Learned
                }).ToList()
            };

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            var tempFile = Path.Combine(Path.GetTempPath(), $"cards_export_{user.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.json");
            await File.WriteAllTextAsync(tempFile, json, cancellationToken);

            try
            {
                // TODO: rewrite into Telegram.BOT api
                var botToken = configuration["Bot:Token"]
                    ?? configuration["Token"]
                    ?? Environment.GetEnvironmentVariable("BOT_TOKEN")
                    ?? throw new InvalidOperationException("BOT_TOKEN not found");

                var httpClient = httpClientFactory.CreateClient();
                var apiUrl = $"https://api.telegram.org/bot{botToken}/sendDocument";

                await using var fileStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read);
                var fileName = $"cards_export_{DateTime.UtcNow:yyyyMMdd}.json";

                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(command.ChatId.ToString()), "chat_id");
                content.Add(new StringContent($"✅ Экспортировано {cards.Count} карточек"), "caption");

                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                content.Add(fileContent, "document", fileName);

                var response = await httpClient.PostAsync(apiUrl, content, cancellationToken);
                response.EnsureSuccessStatusCode();
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
        catch (Exception ex)
        {
            await botClient.SendMessage(
                chatId: command.ChatId,
                text: $"❌ Ошибка при экспорте: {ex.Message}",
                cancellationToken: cancellationToken);
        }
    }
}